// ***********************************************************************
// Assembly         : ServePoint.Cadet
// Author           : Matthew D. Barker
// Created          : 02-09-2026
//
// Last Modified By : Matthew D. Barker
// Last Modified On : 02-13-2026
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
/// Seeds the protected (built-in) admin account and ensures it has Admin role.
/// </summary>
public static class InitializeIdentity
{
    public static async Task RunAsync(IServiceProvider services)
    {
        var config = services.GetRequiredService<IConfiguration>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        var adminEmail = AdminSentinel.GetProtectedAdminEmail(config);

        // Keep compatibility with your existing config key
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

            var create = await userManager.CreateAsync(adminUser, adminPassword);
            if (!create.Succeeded)
            {
                throw new InvalidOperationException(
                    "Failed to create protected admin: " +
                    string.Join("; ", create.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            // Ensure email/username are consistent (idempotent)
            var changed = false;

            if (!string.Equals(adminUser.UserName, adminEmail, StringComparison.OrdinalIgnoreCase))
            {
                adminUser.UserName = adminEmail;
                changed = true;
            }

            if (!string.Equals(adminUser.Email, adminEmail, StringComparison.OrdinalIgnoreCase))
            {
                adminUser.Email = adminEmail;
                changed = true;
            }

            if (!adminUser.EmailConfirmed)
            {
                adminUser.EmailConfirmed = true;
                changed = true;
            }

            if (changed)
            {
                var update = await userManager.UpdateAsync(adminUser);
                if (!update.Succeeded)
                {
                    throw new InvalidOperationException(
                        "Failed to update protected admin: " +
                        string.Join("; ", update.Errors.Select(e => e.Description)));
                }
            }
        }

        // Ensure Admin role membership
        if (!await userManager.IsInRoleAsync(adminUser, Roles.Admin))
        {
            var addRole = await userManager.AddToRoleAsync(adminUser, Roles.Admin);
            if (!addRole.Succeeded)
            {
                throw new InvalidOperationException(
                    "Failed to assign Admin role to protected admin: " +
                    string.Join("; ", addRole.Errors.Select(e => e.Description)));
            }
        }
    }
}
