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
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
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
                // If this fails, your migrations were not applied (or wrong DB)
                await db.Database.ExecuteSqlRawAsync("SELECT 1 FROM \"AspNetRoles\" LIMIT 1");
            }
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("PendingModelChangesWarning"))
        {
            // In Dev: fail fast so you fix migrations
            if (env.IsDevelopment())
                throw new InvalidOperationException(
                    "Database schema is out of date. Run: dotnet ef migrations add <Name> && dotnet ef database update", ex);

            // in Prod: log and continue (Render needs the app to boot)
            // If you want logging here, inject ILogger via scope.ServiceProvider
        }

        if (!await db.Database.CanConnectAsync())
            throw new InvalidOperationException("Database not reachable after migration");

        await InitializeRoles.RunAsync(scope.ServiceProvider);
        await InitializeIdentity.RunAsync(scope.ServiceProvider);
        await InitializeServePoint.RunAsync(scope.ServiceProvider);
        await InitializeDefaultUserRole.RunAsync(scope.ServiceProvider);
    }
}