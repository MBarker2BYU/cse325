// ***********************************************************************
// Assembly       : ServePoint.Cadet
// Author           : Matthew D. Barker
// Created          : 02-04-2026
//
// Last Modified By : Matthew D. Barker
// Last Modified On : 02-04-2026
// ***********************************************************************
// <copyright file="Roles.cs" company="ServePoint.Cadet">
//     Copyright (c) Matthew D. Barker. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using Microsoft.AspNetCore.Identity;

namespace ServePoint.Cadet.Data.Seed;

/// <summary>
/// Class Roles.
/// </summary>
public class Roles
{
    /// <summary>
    /// Seeds the specified services.
    /// </summary>
    /// <param name="services">The services.</param>
    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var role in Auth.Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }
}