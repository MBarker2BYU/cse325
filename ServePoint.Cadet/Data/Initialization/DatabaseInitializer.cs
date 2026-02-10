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

namespace ServePoint.Cadet.Data.Initialization;

/// <summary>
/// Class DatabaseInitializer.
/// </summary>
public static class DatabaseInitializer
{
    /// <summary>
    /// Ensure healthy as an asynchronous operation.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    /// <exception cref="System.InvalidOperationException">Database not reachable</exception>
    public static async Task EnsureHealthyAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (db.Database.IsSqlite())
        {
            var dataPath = Path.Combine(AppContext.BaseDirectory, "Data");
            Directory.CreateDirectory(dataPath);
        }

        // 1. Create database if needed (SQLite-safe)
        try
        {
            await db.Database.MigrateAsync();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("PendingModelChangesWarning"))
        {
            throw new InvalidOperationException(
                "Database schema is out of date. Run: dotnet ef migrations add <Name> && dotnet ef database update", ex);
        }
        
        // 2. Now verify connectivity
        if (!await db.Database.CanConnectAsync())
            throw new InvalidOperationException("Database not reachable after migration");


        await InitializeRoles.RunAsync(scope.ServiceProvider);
        await InitializeIdentity.RunAsync(scope.ServiceProvider);
        await InitializeServePoint.RunAsync(scope.ServiceProvider);
        await InitializeDefaultUserRole.RunAsync(scope.ServiceProvider);
    }
}