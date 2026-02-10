// ***********************************************************************
// Assembly         : ServePoint.Cadet
// Author           : Matthew D. Barker
// Created          : 02-09-2026
//
// Last Modified By : Matthew D. Barker
// Last Modified On : 02-09-2026
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
/// Class InitializeRoles.
/// </summary>
public static class InitializeRoles
{
    /// <summary>
    /// Run as an asynchronous operation.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    /// <exception cref="System.Exception">Failed to create role: {role}</exception>
    public static async Task RunAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(role));
                if (!result.Succeeded)
                    throw new Exception($"Failed to create role: {role}");
            }
        }
    }
}