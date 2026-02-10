// ***********************************************************************
// Assembly         : ServePoint.Cadet
// Author           : Matthew D. Barker
// Created          : 02-10-2026
//
// Last Modified By : Matthew D. Barker
// Last Modified On : 02-10-2026
// ***********************************************************************
// <copyright file="OpportunityManagementService.cs" company="ServePoint.Cadet">
//     Copyright (c) Matthew D. Barker. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using Microsoft.EntityFrameworkCore;
using ServePoint.Cadet.Auth;
using ServePoint.Cadet.Data;
using ServePoint.Cadet.Models.Entities;
using ServePoint.Cadet.Models.Opportunities;
using System.Net;

namespace ServePoint.Cadet.Data.Services;

/// <summary>
/// Class OpportunityManagementService. This class cannot be inherited.
/// </summary>
public sealed class OpportunityManagementService
{
    /// <summary>
    /// The m database
    /// </summary>
    private readonly ApplicationDbContext m_Db;

    /// <summary>
    /// Initializes a new instance of the <see cref="OpportunityManagementService"/> class.
    /// </summary>
    /// <param name="db">The database.</param>
    public OpportunityManagementService(ApplicationDbContext db)
    {
        m_Db = db;
    }

    /// <summary>
    /// Class ActorContext. This class cannot be inherited.
    /// </summary>
    /// <param name="UserId">The user identifier.</param>
    /// <param name="IsOrganizer">if set to <c>true</c> [is organizer].</param>
    /// <param name="IsInstructor">if set to <c>true</c> [is instructor].</param>
    /// <param name="IsAdmin">if set to <c>true</c> [is admin].</param>
    public sealed record ActorContext(
        string UserId,
        bool IsOrganizer,
        bool IsInstructor,
        bool IsAdmin)
    {
        /// <summary>
        /// Gets a value indicating whether this instance can manage.
        /// </summary>
        /// <value><c>true</c> if this instance can manage; otherwise, <c>false</c>.</value>
        public bool CanManage => IsOrganizer || IsInstructor || IsAdmin;
        /// <summary>
        /// Gets a value indicating whether this instance can approve.
        /// </summary>
        /// <value><c>true</c> if this instance can approve; otherwise, <c>false</c>.</value>
        public bool CanApprove => IsInstructor || IsAdmin;
        /// <summary>
        /// Gets a value indicating whether [automatic approve].
        /// </summary>
        /// <value><c>true</c> if [automatic approve]; otherwise, <c>false</c>.</value>
        public bool AutoApprove => IsInstructor || IsAdmin;
    }

    /// <summary>
    /// Class OpportunityRow. This class cannot be inherited.
    /// </summary>
    /// <param name="Id">The identifier.</param>
    /// <param name="Title">The title.</param>
    /// <param name="Date">The date.</param>
    /// <param name="Hours">The hours.</param>
    /// <param name="IsApproved">if set to <c>true</c> [is approved].</param>
    /// <param name="ContactName">Name of the contact.</param>
    /// <param name="ContactEmail">The contact email.</param>
    /// <param name="ContactPhone">The contact phone.</param>
    /// <param name="City">The city.</param>
    /// <param name="ApprovedAtUtc">The approved at UTC.</param>
    /// <param name="ApprovedByUserId">The approved by user identifier.</param>
    /// <param name="CreatedByUserId">The created by user identifier.</param>
    public sealed record OpportunityRow(
        int Id,
        string Title,
        DateTime Date,
        int Hours,
        bool IsApproved,
        string? ContactName,
        string? ContactEmail,
        string? ContactPhone,
        string? City,
        DateTime? ApprovedAtUtc,
        string? ApprovedByUserId,
        string CreatedByUserId);

    ///// <summary>
    ///// Class CreateModel. This class cannot be inherited.
    ///// </summary>
    //public sealed class CreateModel
    //{
    //    /// <summary>
    //    /// Gets or sets the title.
    //    /// </summary>
    //    /// <value>The title.</value>
    //    public string Title { get; set; } = "";
    //    /// <summary>
    //    /// Gets or sets the date.
    //    /// </summary>
    //    /// <value>The date.</value>
    //    public DateTime Date { get; set; } = DateTime.Today;
    //    /// <summary>
    //    /// Gets or sets the hours.
    //    /// </summary>
    //    /// <value>The hours.</value>
    //    public int Hours { get; set; } = 1;
    //    /// <summary>
    //    /// Gets or sets the description.
    //    /// </summary>
    //    /// <value>The description.</value>
    //    public string? Description { get; set; }

    //    /// <summary>
    //    /// Gets or sets the name of the contact.
    //    /// </summary>
    //    /// <value>The name of the contact.</value>
    //    public string ContactName { get; set; } = "";
    //    /// <summary>
    //    /// Gets or sets the contact email.
    //    /// </summary>
    //    /// <value>The contact email.</value>
    //    public string? ContactEmail { get; set; }
    //    /// <summary>
    //    /// Gets or sets the contact phone.
    //    /// </summary>
    //    /// <value>The contact phone.</value>
    //    public string? ContactPhone { get; set; }

    //    /// <summary>
    //    /// Gets or sets the street1.
    //    /// </summary>
    //    /// <value>The street1.</value>
    //    public string Street1 { get; set; } = "";
    //    /// <summary>
    //    /// Gets or sets the street2.
    //    /// </summary>
    //    /// <value>The street2.</value>
    //    public string? Street2 { get; set; }
    //    /// <summary>
    //    /// Gets or sets the city.
    //    /// </summary>
    //    /// <value>The city.</value>
    //    public string City { get; set; } = "";
    //    /// <summary>
    //    /// Gets or sets the state.
    //    /// </summary>
    //    /// <value>The state.</value>
    //    public string State { get; set; } = "";
    //    /// <summary>
    //    /// Gets or sets the postal code.
    //    /// </summary>
    //    /// <value>The postal code.</value>
    //    public string PostalCode { get; set; } = "";
    //}

    ///// <summary>
    ///// Class EditModel. This class cannot be inherited.
    ///// </summary>
    //public sealed class EditModel : CreateModel
    //{
    //    /// <summary>
    //    /// Gets or sets the identifier.
    //    /// </summary>
    //    /// <value>The identifier.</value>
    //    public int Id { get; set; }

    //    // used to enforce "no edit/delete after date" for organizers
    //    /// <summary>
    //    /// Gets or sets a value indicating whether [was approved].
    //    /// </summary>
    //    /// <value><c>true</c> if [was approved]; otherwise, <c>false</c>.</value>
    //    public bool WasApproved { get; set; }
    //    /// <summary>
    //    /// Gets or sets the created by user identifier.
    //    /// </summary>
    //    /// <value>The created by user identifier.</value>
    //    public string CreatedByUserId { get; set; } = "";
    //}

    /// <summary>
    /// Get all as an asynchronous operation.
    /// </summary>
    /// <param name="ct">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>A Task&lt;List`1&gt; representing the asynchronous operation.</returns>
    public async Task<List<OpportunityRow>> GetAllAsync(CancellationToken ct = default)
    {
        // Adjust DbSet/property names if your model differs
        var items = await m_Db.VolunteerOpportunities
            .AsNoTracking()
            .Include(o => o.Contact)
            .ThenInclude(c => c.Address)
            .OrderByDescending(o => o.Date)
            .ToListAsync(ct);

        return items.Select(o => new OpportunityRow(
            o.Id,
            o.Title ?? "",
            o.Date,
            o.Hours,
            o.IsApproved,
            o.Contact?.Name,
            o.Contact?.Email,
            o.Contact?.Phone,
            o.Contact?.Address?.City,
            o.ApprovedAt,
            o.ApprovedByUserId,
            o.CreatedByUserId ?? ""
        )).ToList();
    }

    /// <summary>
    /// Create as an asynchronous operation.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <param name="actor">The actor.</param>
    /// <param name="ct">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>A Task&lt;System.ValueTuple&gt; representing the asynchronous operation.</returns>
    public async Task<(bool Ok, string Message)> CreateAsync(CreateModel input, ActorContext actor, CancellationToken ct = default)
    {
        if (!actor.CanManage) return (false, "Not authorized.");

        var now = DateTime.UtcNow;

        var contact = new Contact
        {
            Name = input.ContactName.Trim(),
            Email = string.IsNullOrWhiteSpace(input.ContactEmail) ? null : input.ContactEmail.Trim(),
            Phone = string.IsNullOrWhiteSpace(input.ContactPhone) ? null : input.ContactPhone.Trim(),
            Address = new Address
            {
                Street1 = input.Street1.Trim(),
                Street2 = string.IsNullOrWhiteSpace(input.Street2) ? null : input.Street2.Trim(),
                City = input.City.Trim(),
                State = input.State.Trim(),
                PostalCode = input.PostalCode.Trim()
            }
        };

        var entity = new VolunteerOpportunity
        {
            Title = input.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(input.Description) ? null : input.Description.Trim(),
            Date = input.Date,
            Hours = input.Hours,

            CreatedByUserId = actor.UserId,
            CreatedAt = now,

            Contact = contact,

            // Approval rules:
            IsApproved = actor.AutoApprove,
            ApprovedAt = actor.AutoApprove ? now : null,
            ApprovedByUserId = actor.AutoApprove ? actor.UserId : null
        };

        m_Db.VolunteerOpportunities.Add(entity);
        await m_Db.SaveChangesAsync(ct);

        return (true, actor.AutoApprove ? "Opportunity created and approved." : "Opportunity created (pending approval).");
    }

    /// <summary>
    /// Update as an asynchronous operation.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <param name="actor">The actor.</param>
    /// <param name="ct">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>A Task&lt;System.ValueTuple&gt; representing the asynchronous operation.</returns>
    public async Task<(bool Ok, string Message)> UpdateAsync(EditModel input, ActorContext actor, CancellationToken ct = default)
    {
        if (!actor.CanManage) return (false, "Not authorized.");

        var existing = await m_Db.VolunteerOpportunities
            .Include(o => o.Contact)
            .ThenInclude(c => c.Address)
            .FirstOrDefaultAsync(o => o.Id == input.Id, ct);

        if (existing is null) return (false, "Opportunity not found.");

        // Organizer rule: can edit only until opportunity date (and typically only their own, if you want it)
        if (actor.IsOrganizer && !actor.CanApprove)
        {
            if (existing.Date.Date < DateTime.Today)
                return (false, "This opportunity has already passed and cannot be edited.");

            // Optional ownership rule (enable if you want):
            // if (!string.Equals(existing.CreatedByUserId, actor.UserId, StringComparison.Ordinal))
            //     return (false, "Organizers can only edit opportunities they created.");
        }

        existing.Title = input.Title.Trim();
        existing.Description = string.IsNullOrWhiteSpace(input.Description) ? null : input.Description.Trim();
        existing.Date = input.Date;

        // Hours: Only Instructor/Admin can change hours once created
        if (actor.CanApprove)
            existing.Hours = input.Hours;

        existing.Contact ??= new Contact();
        existing.Contact.Name = input.ContactName.Trim();
        existing.Contact.Email = string.IsNullOrWhiteSpace(input.ContactEmail) ? null : input.ContactEmail.Trim();
        existing.Contact.Phone = string.IsNullOrWhiteSpace(input.ContactPhone) ? null : input.ContactPhone.Trim();

        // Address (optional)
        var hasAnyAddress =
            !string.IsNullOrWhiteSpace(input.Street1) ||
            !string.IsNullOrWhiteSpace(input.City) ||
            !string.IsNullOrWhiteSpace(input.PostalCode);

        if (!hasAnyAddress)
        {
            existing.Contact.AddressId = null;
            existing.Contact.Address = null;
        }
        else
        {
            existing.Contact.Address ??= new Address();
            existing.Contact.Address.Street1 = input.Street1.Trim();
            existing.Contact.Address.Street2 = string.IsNullOrWhiteSpace(input.Street2) ? null : input.Street2.Trim();
            existing.Contact.Address.City = input.City.Trim();
            existing.Contact.Address.State = input.State.Trim();
            existing.Contact.Address.PostalCode = input.PostalCode.Trim();
        }

        // Approval behavior:
        // - Instructor/Admin: stays approved (auto approve)
        // - Organizer: any edit returns to pending (must be reapproved)
        var now = DateTime.UtcNow;
        if (actor.AutoApprove)
        {
            existing.IsApproved = true;
            existing.ApprovedAt = now;
            existing.ApprovedByUserId = actor.UserId;
        }
        else
        {
            existing.IsApproved = false;
            existing.ApprovedAt = null;
            existing.ApprovedByUserId = null;
        }

        await m_Db.SaveChangesAsync(ct);
        return (true, actor.AutoApprove ? "Saved changes (approved)." : "Saved changes (pending re-approval).");
    }

    /// <summary>
    /// Delete as an asynchronous operation.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="actor">The actor.</param>
    /// <param name="ct">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>A Task&lt;System.ValueTuple&gt; representing the asynchronous operation.</returns>
    public async Task<(bool Ok, string Message)> DeleteAsync(int id, ActorContext actor, CancellationToken ct = default)
    {
        if (!actor.CanManage) return (false, "Not authorized.");

        var existing = await m_Db.VolunteerOpportunities.FirstOrDefaultAsync(o => o.Id == id, ct);
        if (existing is null) return (false, "Opportunity not found.");

        if (actor.IsOrganizer && !actor.CanApprove)
        {
            if (existing.Date.Date < DateTime.Today)
                return (false, "This opportunity has already passed and cannot be deleted.");

            // Optional ownership rule:
            // if (!string.Equals(existing.CreatedByUserId, actor.UserId, StringComparison.Ordinal))
            //     return (false, "Organizers can only delete opportunities they created.");
        }

        m_Db.VolunteerOpportunities.Remove(existing);
        await m_Db.SaveChangesAsync(ct);
        return (true, "Opportunity deleted.");
    }

    /// <summary>
    /// Approve as an asynchronous operation.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="actor">The actor.</param>
    /// <param name="ct">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>A Task&lt;System.ValueTuple&gt; representing the asynchronous operation.</returns>
    public async Task<(bool Ok, string Message)> ApproveAsync(int id, ActorContext actor, CancellationToken ct = default)
    {
        if (!actor.CanApprove) return (false, "Not authorized.");

        var existing = await m_Db.VolunteerOpportunities.FirstOrDefaultAsync(o => o.Id == id, ct);
        if (existing is null) return (false, "Opportunity not found.");

        if (existing.IsApproved) return (true, "Already approved.");

        existing.IsApproved = true;
        existing.ApprovedAt = DateTime.UtcNow;
        existing.ApprovedByUserId = actor.UserId;

        await m_Db.SaveChangesAsync(ct);
        return (true, "Opportunity approved.");
    }

    /// <summary>
    /// Unapprove as an asynchronous operation.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="actor">The actor.</param>
    /// <param name="ct">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>A Task&lt;System.ValueTuple&gt; representing the asynchronous operation.</returns>
    public async Task<(bool Ok, string Message)> UnapproveAsync(int id, ActorContext actor, CancellationToken ct = default)
    {
        if (!actor.CanApprove) return (false, "Not authorized.");

        var existing = await m_Db.VolunteerOpportunities.FirstOrDefaultAsync(o => o.Id == id, ct);
        if (existing is null) return (false, "Opportunity not found.");

        if (!existing.IsApproved) return (true, "Already pending.");

        existing.IsApproved = false;
        existing.ApprovedAt = null;
        existing.ApprovedByUserId = null;

        await m_Db.SaveChangesAsync(ct);
        return (true, "Opportunity set to pending.");
    }
}
