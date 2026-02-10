// ***********************************************************************
// Assembly         : ServePoint.Cadet
// Author           : Matthew D. Barker
// Created          : 02-09-2026
//
// Last Modified By : Matthew D. Barker
// Last Modified On : 02-09-2026
// ***********************************************************************
// <copyright file="InitializeIdentity.cs" company="ServePoint.Cadet">
//     Copyright (c) Matthew D. Barker. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using Microsoft.AspNetCore.Identity;
using ServePoint.Cadet.Auth;

namespace ServePoint.Cadet.Data.Initialization;

/// <summary>
/// Class InitializeIdentity.
/// </summary>
public static class InitializeIdentity
{
    /// <summary>
    /// Run as an asynchronous operation.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    /// <exception cref="System.InvalidOperationException">Missing DefaultAdmin:Password</exception>
    /// <exception cref="System.Exception">Failed to create protected admin</exception>
    public static async Task RunAsync(IServiceProvider services)
    {
        var config = services.GetRequiredService<IConfiguration>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        var adminEmail = AdminSentinel.GetProtectedAdminEmail(config);
        var adminPassword = config["DefaultAdmin:Password"]
                            ?? throw new InvalidOperationException("Missing DefaultAdmin:Password");

        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (!result.Succeeded)
                throw new Exception("Failed to create protected admin");
        }

        if (!await userManager.IsInRoleAsync(adminUser, Roles.Admin))
        {
            await userManager.AddToRoleAsync(adminUser, Roles.Admin);
        }
    }
}