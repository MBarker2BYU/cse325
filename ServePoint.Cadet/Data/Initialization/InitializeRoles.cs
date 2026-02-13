// ***********************************************************************
// Assembly         : ServePoint.Cadet
// Author           : Matthew D. Barker
// Created          : 02-09-2026
//
// Last Modified By : Matthew D. Barker
// Last Modified On : 02-13-2026
// ***********************************************************************
// <copyright file="InitializeRoles.cs" company="ServePoint.Cadet">
//     Copyright (c) Matthew D. Barker. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using Microsoft.AspNetCore.Identity;
using ServePoint.Cadet.Auth;

namespace ServePoint.Cadet.Data.Initialization;

/// <summary>
/// Ensures all application roles exist (idempotent).
/// Safe for repeated execution.
/// </summary>
public static class InitializeRoles
{
    public static async Task RunAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var roleName in Roles.All)
        {
            var normalized = roleManager.NormalizeKey(roleName);

            var exists = await roleManager.RoleExistsAsync(roleName);
            if (exists)
                continue;

            var role = new IdentityRole
            {
                Name = roleName,
                NormalizedName = normalized
            };

            var result = await roleManager.CreateAsync(role);

            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));

                throw new InvalidOperationException(
                    $"Failed to create role '{roleName}'. Errors: {errors}");
            }
        }
    }
}