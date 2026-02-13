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
    private readonly IDbContextFactory<ApplicationDbContext> m_DbFactory;
    private readonly UserManager<ApplicationUser> m_UserManager;
    private readonly RoleManager<IdentityRole> m_RoleManager;
    private readonly IConfiguration m_Config;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserManagementService"/> class.
    /// </summary>
    public UserManagementService(
        IDbContextFactory<ApplicationDbContext> dbFactory,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration config)
    {
        m_DbFactory = dbFactory;
        m_UserManager = userManager;
        m_RoleManager = roleManager;
        m_Config = config;
    }

    public sealed record UserRow(
        string Id,
        string? Email,
        string? UserName,
        List<string> Roles,
        DateTimeOffset? LockoutEndUtc);

    /// <summary>
    /// Load as an asynchronous operation.
    /// </summary>
    public async Task<(List<UserRow> Rows, string? CurrentUserId, string ProtectedAdminEmail, int AdminCount)> LoadAsync(
        string? currentUserId,
        CancellationToken ct = default)
    {
        var protectedAdminEmail = AdminSentinel.GetProtectedAdminEmail(m_Config);

        // Count admins once (used for "last admin" rule)
        var adminCount = (await m_UserManager.GetUsersInRoleAsync(Roles.Admin)).Count;

        // Materialize users list (safe)
        var users = await m_UserManager.Users
            .AsNoTracking()
            .OrderBy(u => u.Email)
            .Select(u => new { u.Id, u.Email, u.UserName, u.LockoutEnd })
            .ToListAsync(ct);

        // Pull role mappings via a fresh DbContext (prevents "second operation on this context" issues)
        await using var db = await m_DbFactory.CreateDbContextAsync(ct);

        var userRoles = await (
            from ur in db.UserRoles
            join r in db.Roles on ur.RoleId equals r.Id
            select new { ur.UserId, RoleName = r.Name! }
        ).AsNoTracking().ToListAsync(ct);

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
            if (AdminSentinel.IsProtectedAdmin(user.Email, m_Config))
                return (false, $"You cannot remove Admin from the built-in admin account ({AdminSentinel.GetProtectedAdminEmail(m_Config)}).");

            if (!string.IsNullOrWhiteSpace(currentUserId) &&
                string.Equals(currentUserId, userId, StringComparison.Ordinal))
                return (false, "You cannot remove Admin from yourself.");

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

        await m_UserManager.ResetAccessFailedCountAsync(user);

        return (true, $"Unsuspended {(user.Email ?? user.UserName)}.");
    }
}
