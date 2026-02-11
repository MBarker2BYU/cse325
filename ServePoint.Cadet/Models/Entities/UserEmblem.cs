// ***********************************************************************
// Assembly         : ServePoint.Cadet
// Author           : Matthew D. Barker
// Created          : 02-11-2026
//
// Last Modified By : Matthew D. Barker
// Last Modified On : 02-11-2026
// ***********************************************************************
// <copyright file="UserEmblem.cs" company="ServePoint.Cadet">
//     Copyright (c) Matthew D. Barker. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.ComponentModel.DataAnnotations;

namespace ServePoint.Cadet.Models.Entities;

/// <summary>
/// Class UserEmblem.
/// </summary>
public class UserEmblem
{
    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    /// <value>The user identifier.</value>
    [Key]
    public string UserId { get; set; } = default!;
    /// <summary>
    /// Gets or sets the emblem.
    /// </summary>
    /// <value>The emblem.</value>
    public ServiceEmblem Emblem { get; set; } = ServiceEmblem.None;
}