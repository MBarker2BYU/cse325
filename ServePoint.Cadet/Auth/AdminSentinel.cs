// ***********************************************************************
// Assembly         : ServePoint.Cadet
// Author           : Matthew D. Barker
// Created          : 02-09-2026
//
// Last Modified By : Matthew D. Barker
// Last Modified On : 02-05-2026
// ***********************************************************************
// <copyright file="AdminSentinel.cs" company="ServePoint.Cadet">
//     Copyright (c) Matthew D. Barker. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace ServePoint.Cadet.Auth;

/// <summary>
/// Class AdminSentinel.
/// </summary>
public class AdminSentinel
{
    /// <summary>
    /// Gets the protected admin email.
    /// </summary>
    /// <param name="config">The configuration.</param>
    /// <returns>System.String.</returns>
    /// <exception cref="System.InvalidOperationException">Missing config: DefaultAdmin:Email</exception>
    public static string GetProtectedAdminEmail(IConfiguration config)
        => config["DefaultAdmin:Email"]
           ?? throw new InvalidOperationException("Missing config: DefaultAdmin:Email");

    /// <summary>
    /// Determines whether [is protected admin] [the specified user email].
    /// </summary>
    /// <param name="userEmail">The user email.</param>
    /// <param name="config">The configuration.</param>
    /// <returns><c>true</c> if [is protected admin] [the specified user email]; otherwise, <c>false</c>.</returns>
    public static bool IsProtectedAdmin(string? userEmail, IConfiguration config)
        => !string.IsNullOrWhiteSpace(userEmail)
           && string.Equals(userEmail, GetProtectedAdminEmail(config), StringComparison.OrdinalIgnoreCase);
}