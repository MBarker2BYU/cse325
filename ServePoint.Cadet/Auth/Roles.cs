

// ***********************************************************************
// Assembly         : ServePoint.Cadet
// Author           : Matthew D. Barker
// Created          : 02-04-2026
//
// Last Modified By : Matthew D. Barker
// Last Modified On : 02-04-2026
// ***********************************************************************
// <copyright file="Roles.cs" company="ServePoint.Cadet">
//     Copyright (c) Matthew D. Barker. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
namespace ServePoint.Cadet.Auth;

/// <summary>
/// Class Roles.
/// </summary>
public static class Roles
{
    /// <summary>
    /// The user
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public const string User = "User";
    /// <summary>
    /// The organizer
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public const string Organizer = "Organizer";
    /// <summary>
    /// The instructor
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public const string Instructor = "Instructor";
    /// <summary>
    /// The admin
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public const string Admin = "Admin";

    /// <summary>
    /// The opportunity creators
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public const string OpportunityCreators = Organizer + "," + Instructor + "," + Admin;

    /// <summary>
    /// All
    /// </summary>
    public static readonly string[] All =
    {
        User,
        Organizer,
        Instructor,
        Admin
    };
}