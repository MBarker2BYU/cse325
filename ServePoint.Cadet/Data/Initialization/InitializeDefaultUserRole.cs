// ***********************************************************************
// Assembly         : ServePoint.Cadet
// Author           : Matthew D. Barker
// Created          : 02-09-2026
//
// Last Modified By : Matthew D. Barker
// Last Modified On : 02-09-2026
// ***********************************************************************
// <copyright file="InitializeDefaultUserRole.cs" company="ServePoint.Cadet">
//     Copyright (c) Matthew D. Barker. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using Microsoft.AspNetCore.Identity;
using ServePoint.Cadet.Auth;

namespace ServePoint.Cadet.Data.Initialization;

/// <summary>
/// Class InitializeDefaultUserRole.
/// </summary>
public static class InitializeDefaultUserRole
{
    /// <summary>
    /// Run as an asynchronous operation.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    /// <exception cref="System.Exception">Failed to assign default User role to {user.Email ?? user.Id}</exception>
    public static async Task RunAsync(IServiceProvider services)
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        // Enumerate users directly from the store (safe for SQLite/Postgres)
        foreach (var user in userManager.Users)
        {
            var roles = await userManager.GetRolesAsync(user);

            if (roles.Count == 0)
            {
                var result = await userManager.AddToRoleAsync(user, Roles.User);
                if (!result.Succeeded)
                {
                    throw new Exception(
                        $"Failed to assign default User role to {user.Email ?? user.Id}");
                }
            }
        }
    }
}