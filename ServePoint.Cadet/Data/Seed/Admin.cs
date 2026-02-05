using Microsoft.AspNetCore.Identity;

namespace ServePoint.Cadet.Data.Seed;

public static class Admin
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var userManager = services.GetRequiredService<UserManager<ServePointCadetUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // Ensure roles exist (defensive)
        foreach (var role in Auth.Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var config = services.GetRequiredService<IConfiguration>();

        var adminEmail = config["DefaultAdmin:Email"]
                         ?? throw new InvalidOperationException("Missing config: DefaultAdmin:Email");

        var adminPassword = config["DefaultAdmin:Password"]
                            ?? throw new InvalidOperationException("Missing config: DefaultAdmin:Password");

        var admin = await userManager.FindByEmailAsync(adminEmail);

        if (admin != null)
            return; // already seeded

        admin = new ServePointCadetUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(admin, adminPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join(" | ", result.Errors.Select(e => e.Description));
            throw new Exception($"Failed to create default admin: {errors}");
        }

        // Assign roles
        await userManager.AddToRoleAsync(admin, Auth.Roles.Admin);
        await userManager.AddToRoleAsync(admin, Auth.Roles.User);
    }
}