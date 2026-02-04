using Microsoft.AspNetCore.Identity;
using ServePoint.Cadet.Auth;

namespace ServePoint.Cadet.Data.Seed;

public class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }
}