using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ServePoint.Cadet.Auth;

namespace ServePoint.Cadet.Data.Initialization;

/// <summary>
/// Ensures every non-staff account has at least the default User role.
/// Instructors and Admins are NOT modified.
/// </summary>
public static class InitializeDefaultUserRole
{
    public static async Task RunAsync(IServiceProvider services)
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        // Materialize users so the query is completed before we start role operations
        // (prevents provider/DbContext concurrency weirdness)
        var users = await userManager.Users
            .AsNoTracking()
            .Select(u => new { u.Id })
            .ToListAsync();

        foreach (var u in users)
        {
            var user = await userManager.FindByIdAsync(u.Id);
            if (user is null)
                continue;

            var roles = await userManager.GetRolesAsync(user);

            // Skip staff accounts (they should never be auto-modified)
            if (roles.Contains(Roles.Admin) || roles.Contains(Roles.Instructor))
                continue;

            // If user has no roles at all, assign default User role
            if (roles.Count == 0)
            {
                var result = await userManager.AddToRoleAsync(user, Roles.User);

                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Failed to assign default User role to {user.Email ?? user.Id}: " +
                        string.Join("; ", result.Errors.Select(e => e.Description)));
                }
            }
        }
    }
}