using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace ServePoint.Cadet.Data.Initialization;

public static class DatabaseInitializer
{
    public static async Task EnsureHealthyAsync(IServiceProvider root, IHostEnvironment env)
    {
        // 1) Migrate database schema
        await using (var scope = root.CreateAsyncScope())
        {
            var sp = scope.ServiceProvider;
            var db = sp.GetRequiredService<ApplicationDbContext>();

            await db.Database.MigrateAsync();

            if (!await db.Database.CanConnectAsync())
                throw new InvalidOperationException("Database not reachable after migration.");
        }

        // 2) Seed Identity (roles first, then admin)
        await using (var scope = root.CreateAsyncScope())
        {
            var sp = scope.ServiceProvider;
            await InitializeRoles.RunAsync(sp);
            await InitializeIdentity.RunAsync(sp);
        }

        // 3) Seed application/domain data
        await using (var scope = root.CreateAsyncScope())
        {
            var sp = scope.ServiceProvider;
            await InitializeServePoint.RunAsync(sp);
        }

        // 4) Apply default role policy (if used)
        await using (var scope = root.CreateAsyncScope())
        {
            var sp = scope.ServiceProvider;
            await InitializeDefaultUserRole.RunAsync(sp);
        }
    }
}