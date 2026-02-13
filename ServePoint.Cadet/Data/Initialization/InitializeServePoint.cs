// ***********************************************************************
// Assembly         : ServePoint.Cadet
// Author           : Matthew D. Barker
// Created          : 02-09-2026
//
// Last Modified By : Matthew D. Barker
// Last Modified On : 02-13-2026
// ***********************************************************************
// <copyright file="InitializeServePoint.cs" company="ServePoint.Cadet">
//     Copyright (c) Matthew D. Barker. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using Microsoft.EntityFrameworkCore;

namespace ServePoint.Cadet.Data.Initialization;

/// <summary>
/// Seeds ServePoint domain data (idempotent).
/// Keep all inserts guarded by existence checks.
/// </summary>
public static class InitializeServePoint
{
    public static async Task RunAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        
        await Task.CompletedTask;
    }
}