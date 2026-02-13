// ***********************************************************************
// Assembly         : ServePoint.Cadet
// Author           : Matthew D. Barker
// Created          : 02-09-2026
//
// Last Modified By : Matthew D. Barker
// Last Modified On : 02-10-2026
// ***********************************************************************
// <copyright file="ApplicationDbContext.cs" company="ServePoint.Cadet">
//     Copyright (c) Matthew D. Barker. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ServePoint.Cadet.Models.Entities;
using System.Reflection.Emit;

namespace ServePoint.Cadet.Data;

/// <summary>
/// Class ApplicationDbContext.
/// Implements the <see cref="Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext{ServePoint.Cadet.Data.ApplicationUser}" />
/// </summary>
/// <param name="options">The options.</param>
/// <seealso cref="Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext{ServePoint.Cadet.Data.ApplicationUser}" />
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        var boolToInt = new BoolToZeroOneConverter<int>();

        builder.Entity<VolunteerOpportunity>(entity =>
        {
            entity.Property(x => x.IsApproved)
                .HasConversion(boolToInt);

            entity.Property(x => x.IsDeletionRequested)
                .HasConversion(boolToInt);
        });

        builder.Entity<VolunteerSignup>(entity =>
        {
            entity.Property(x => x.AttendanceSubmitted)
                .HasConversion(boolToInt);

            entity.Property(x => x.AttendanceApproved)
                .HasConversion(boolToInt);
        });

    }

    /// <summary>
    /// Gets the volunteer opportunities.
    /// </summary>
    /// <value>The volunteer opportunities.</value>
    public DbSet<VolunteerOpportunity> VolunteerOpportunities => Set<VolunteerOpportunity>();
    /// <summary>
    /// Gets the contacts.
    /// </summary>
    /// <value>The contacts.</value>
    public DbSet<Contact> Contacts => Set<Contact>();
    /// <summary>
    /// Gets the addresses.
    /// </summary>
    /// <value>The addresses.</value>
    public DbSet<Address> Addresses => Set<Address>();
    /// <summary>
    /// Gets the volunteer signups.
    /// </summary>
    /// <value>The volunteer signups.</value>
    public DbSet<VolunteerSignup> VolunteerSignups => Set<VolunteerSignup>();

    /// <summary>
    /// Gets the user emblems.
    /// </summary>
    /// <value>The user emblems.</value>
    public DbSet<UserEmblem> UserEmblems => Set<UserEmblem>();
}