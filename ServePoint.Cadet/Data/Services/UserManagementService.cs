// ***********************************************************************
// Assembly         : ServePoint.Cadet
// Author           : Matthew D. Barker
// Created          : 02-10-2026
//
// Last Modified By : Matthew D. Barker
// Last Modified On : 02-10-2026
// ***********************************************************************
// <copyright file="UserManagementService.cs" company="ServePoint.Cadet">
//     Copyright (c) Matthew D. Barker. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ServePoint.Cadet.Auth;

namespace ServePoint.Cadet.Data.Services;

/// <summary>
/// Class UserManagementService. This class cannot be inherited.
/// </summary>
public sealed class UserManagementService
{
    /// <summary>
    /// The database
    /// </summary>
    private readonly ApplicationDbContext m_Db;
    /// <summary>
    /// The user manager
    /// </summary>
    private readonly UserManager<ApplicationUser> m_UserManager;
    /// <summary>
    /// The role manager
    /// </summary>
    private readonly RoleManager<IdentityRole> m_RoleManager;
    /// <summary>
    /// The configuration
    /// </summary>
    private readonly IConfiguration m_Config;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserManagementService"/> class.
    /// </summary>
    /// <param name="db">The database.</param>
    /// <param name="userManager">The user manager.</param>
    /// <param name="roleManager">The role manager.</param>
    /// <param name="config">The configuration.</param>
    public UserManagementService(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration config)
    {
        m_Db = db;
        m_UserManager = userManager;
        m_RoleManager = roleManager;
        m_Config = config;
    }

    /// <summary>
    /// Class UserRow. This class cannot be inherited.
    /// </summary>
    /// <param name="Id">The identifier.</param>
    /// <param name="Email">The email.</param>
    /// <param name="UserName">Name of the user.</param>
    /// <param name="Roles">The roles.</param>
    /// <param name="LockoutEndUtc">The lockout end UTC.</param>
    public sealed record UserRow(
        string Id,
        string? Email,
        string? UserName,
        List<string> Roles,
        DateTimeOffset? LockoutEndUtc);


    /// <summary>
    /// Load as an asynchronous operation.
    /// </summary>
    /// <param name="currentUserId">The current user identifier.</param>
    /// <param name="ct">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>A Task&lt;System.ValueTuple&gt; representing the asynchronous operation.</returns>
    public async Task<(List<UserRow> Rows, string? CurrentUserId, string ProtectedAdminEmail, int AdminCount)> LoadAsync(
        string? currentUserId,
        CancellationToken ct = default)
    {
        var protectedAdminEmail = AdminSentinel.GetProtectedAdminEmail(m_Config);

        // Count admins once (used for "last admin" rule)
        var adminCount = (await m_UserManager.GetUsersInRoleAsync(Roles.Admin)).Count;

        var users = await m_UserManager.Users
            .AsNoTracking()
            .OrderBy(u => u.Email)
            .Select(u => new { u.Id, u.Email, u.UserName, u.LockoutEnd })
            .ToListAsync(ct);

        // Single query for all user->role names
        var userRoles = await (
            from ur in m_Db.UserRoles
            join r in m_Db.Roles on ur.RoleId equals r.Id
            select new { ur.UserId, RoleName = r.Name! }
        ).ToListAsync(ct);

        var roleLookup = userRoles
            .GroupBy(x => x.UserId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.RoleName).ToList());

        var rows = new List<UserRow>(users.Count);
        foreach (var u in users)
        {
            roleLookup.TryGetValue(u.Id, out var roles);

            rows.Add(new UserRow(
                u.Id,
                u.Email,
                u.UserName,
                roles ?? [],
                u.LockoutEnd));
        }

        return (rows, currentUserId, protectedAdminEmail, adminCount);
    }

    /// <summary>
    /// Add role as an asynchronous operation.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="role">The role.</param>
    /// <param name="ct">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>A Task&lt;System.ValueTuple&gt; representing the asynchronous operation.</returns>
    public async Task<(bool Ok, string Message)> AddRoleAsync(string userId, string role, CancellationToken ct = default)
    {
        if (!await m_RoleManager.RoleExistsAsync(role))
            return (false, $"Role '{role}' does not exist.");

        var user = await m_UserManager.FindByIdAsync(userId);
        if (user is null)
            return (false, "User not found.");

        if (await m_UserManager.IsInRoleAsync(user, role))
            return (true, $"User already has role '{role}'.");

        var result = await m_UserManager.AddToRoleAsync(user, role);
        if (!result.Succeeded)
            return (false, string.Join(" | ", result.Errors.Select(e => e.Description)));

        return (true, $"Added '{role}' to {(user.Email ?? user.UserName)}.");
    }

    /// <summary>
    /// Remove role as an asynchronous operation.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="role">The role.</param>
    /// <param name="currentUserId">The current user identifier.</param>
    /// <param name="adminCount">The admin count.</param>
    /// <param name="ct">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>A Task&lt;System.ValueTuple&gt; representing the asynchronous operation.</returns>
    public async Task<(bool Ok, string Message)> RemoveRoleAsync(
        string userId,
        string role,
        string? currentUserId,
        int adminCount,
        CancellationToken ct = default)
    {
        var user = await m_UserManager.FindByIdAsync(userId);
        if (user is null)
            return (false, "User not found.");

        if (!await m_UserManager.IsInRoleAsync(user, role))
            return (true, $"User does not have role '{role}'.");

        if (role.Equals(Roles.Admin, StringComparison.OrdinalIgnoreCase))
        {
            // Built-in admin can never lose Admin
            if (AdminSentinel.IsProtectedAdmin(user.Email, m_Config))
                return (false, $"You cannot remove Admin from the built-in admin account ({AdminSentinel.GetProtectedAdminEmail(m_Config)}).");

            // Admin cannot remove Admin from themselves
            if (!string.IsNullOrWhiteSpace(currentUserId) &&
                string.Equals(currentUserId, userId, StringComparison.Ordinal))
                return (false, "You cannot remove Admin from yourself.");

            // Never remove the last admin
            if (adminCount <= 1)
                return (false, "You must have at least one Admin account.");
        }

        var result = await m_UserManager.RemoveFromRoleAsync(user, role);
        if (!result.Succeeded)
            return (false, string.Join(" | ", result.Errors.Select(e => e.Description)));

        return (true, $"Removed '{role}' from {(user.Email ?? user.UserName)}.");
    }

    public async Task<(bool Ok, string Message)> SuspendAsync(string userId, string? currentUserId, CancellationToken ct = default)
    {
        if (!string.IsNullOrWhiteSpace(currentUserId) &&
            string.Equals(currentUserId, userId, StringComparison.Ordinal))
            return (false, "You cannot suspend yourself.");

        var user = await m_UserManager.FindByIdAsync(userId);
        if (user is null)
            return (false, "User not found.");

        // Optional: don’t allow suspending protected admin (recommended)
        if (AdminSentinel.IsProtectedAdmin(user.Email, m_Config))
            return (false, "You cannot suspend the built-in admin account.");

        await m_UserManager.SetLockoutEnabledAsync(user, true);
        var result = await m_UserManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);

        if (!result.Succeeded)
            return (false, string.Join(" | ", result.Errors.Select(e => e.Description)));

        return (true, $"Suspended {(user.Email ?? user.UserName)}.");
    }

    public async Task<(bool Ok, string Message)> UnsuspendAsync(string userId, CancellationToken ct = default)
    {
        var user = await m_UserManager.FindByIdAsync(userId);
        if (user is null)
            return (false, "User not found.");

        var result = await m_UserManager.SetLockoutEndDateAsync(user, null);

        if (!result.Succeeded)
            return (false, string.Join(" | ", result.Errors.Select(e => e.Description)));

        // Optional, but nice:
        await m_UserManager.ResetAccessFailedCountAsync(user);

        return (true, $"Unsuspended {(user.Email ?? user.UserName)}.");
    }

}