// ***********************************************************************
// Assembly         : ServePoint.Cadet
// Author           : Matthew D. Barker
// Created          : 02-09-2026
//
// Last Modified By : Matthew D. Barker
// Last Modified On : 02-09-2026
// ***********************************************************************
// <copyright file="InitializeServePoint.cs" company="ServePoint.Cadet">
//     Copyright (c) Matthew D. Barker. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace ServePoint.Cadet.Data.Initialization;

/// <summary>
/// Class InitializeServePoint.
/// </summary>
public static class InitializeServePoint
{
    /// <summary>
    /// Run as an asynchronous operation.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public static async Task RunAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();

        // Example:
        // if (!await db.OpportunityStatuses.AnyAsync())
        // {
        //     db.OpportunityStatuses.AddRange(...);
        //     await db.SaveChangesAsync();
        // }
    }
}