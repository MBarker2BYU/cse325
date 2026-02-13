// ***********************************************************************
// Assembly         : ServePoint.Cadet
// Author           : Matthew D. Barker
// Created          : 02-09-2026
//
// Last Modified By : Matthew D. Barker
// Last Modified On : 02-09-2026
// ***********************************************************************
// <copyright file="DatabaseInitializer.cs" company="ServePoint.Cadet">
//     Copyright (c) Matthew D. Barker. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ServePoint.Cadet.Data.Initialization;


public static class DatabaseInitializer
{
    public static async Task EnsureHealthyAsync(IServiceProvider services, IHostEnvironment env)
    {
        // 1) Migrate in its own scope
        await RunInNewScopeAsync(services, async sp =>
        {
            var db = sp.GetRequiredService<ApplicationDbContext>();

            if (db.Database.IsSqlite())
            {
                var dataPath = Path.Combine(AppContext.BaseDirectory, "Data");
                Directory.CreateDirectory(dataPath);
            }

            try
            {
                await db.Database.MigrateAsync();

                if (!db.Database.IsSqlite())
                {
                    await db.Database.ExecuteSqlRawAsync("SELECT 1 FROM \"AspNetRoles\" LIMIT 1");
                }
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("PendingModelChangesWarning"))
            {
                if (env.IsDevelopment())
                    throw new InvalidOperationException(
                        "Database schema is out of date. Run: dotnet ef migrations add <Name> && dotnet ef database update", ex);

                // In Prod: ignore (you already set ConfigureWarnings in Program.cs)
            }

            if (!await db.Database.CanConnectAsync())
                throw new InvalidOperationException("Database not reachable after migration");
        });

        // 2) Seed steps in separate scopes (prevents overlapping commands on the same connection)
        await RunInNewScopeAsync(services, sp => InitializeRoles.RunAsync(sp));
        await RunInNewScopeAsync(services, sp => InitializeIdentity.RunAsync(sp));
        await RunInNewScopeAsync(services, sp => InitializeServePoint.RunAsync(sp));
        await RunInNewScopeAsync(services, sp => InitializeDefaultUserRole.RunAsync(sp));
    }

    private static async Task RunInNewScopeAsync(IServiceProvider root, Func<IServiceProvider, Task> work)
    {
        using var scope = root.CreateScope();
        await work(scope.ServiceProvider);
    }
}
