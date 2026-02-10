using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ServePoint.Cadet.Security;

namespace ServePoint.Cadet.Data;

public static class DbSeeder
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;

        var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("SEED");
        var db = sp.GetRequiredService<ApplicationDbContext>();

        // Self-heal schema (safe in dev/prod)
        await db.Database.MigrateAsync();

        // Roles
        var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in AppRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(role));
                if (!result.Succeeded)
                    throw new InvalidOperationException($"Create role '{role}' failed: " +
                        string.Join("; ", result.Errors.Select(e => e.Description)));

                logger.LogInformation("Created role {Role}", role);
            }
        }

        // Built-in admin
        var config = sp.GetRequiredService<IConfiguration>();
        var adminEmail = config["SeedAdmin:Email"] ?? "admin@servepointcadet.local";
        var adminPassword = config["SeedAdmin:Password"] ?? "ChangeMe!123";

        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
        var admin = await userManager.FindByEmailAsync(adminEmail);

        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var create = await userManager.CreateAsync(admin, adminPassword);
            if (!create.Succeeded)
                throw new InvalidOperationException("Create admin failed: " +
                        string.Join("; ", create.Errors.Select(e => e.Description)));

            logger.LogInformation("Created built-in admin {Email}", adminEmail);
        }

        // Ensure roles on admin (idempotent)
        foreach (var role in new[] { AppRoles.User, AppRoles.Organizer, AppRoles.Instructor, AppRoles.Admin })
        {
            if (!await userManager.IsInRoleAsync(admin, role))
            {
                var add = await userManager.AddToRoleAsync(admin, role);
                if (!add.Succeeded)
                    throw new InvalidOperationException($"Add role '{role}' failed: " +
                        string.Join("; ", add.Errors.Select(e => e.Description)));

                logger.LogInformation("Added {Role} to {Email}", role, adminEmail);
            }
        }
    }
}
