using Microsoft.EntityFrameworkCore;
using ServePoint.Cadet.Models.Entities;
using ServePoint.Cadet.Models.Opportunities;

namespace ServePoint.Cadet.Data.Services;

public sealed class OpportunityManagementService(ApplicationDbContext db)
{
    public sealed record ActorContext(
        string UserId,
        bool IsOrganizer,
        bool IsInstructor,
        bool IsAdmin)
    {
        public bool CanManage => IsOrganizer || IsInstructor || IsAdmin;
        public bool CanApprove => IsInstructor || IsAdmin;
        public bool AutoApprove => IsInstructor || IsAdmin;

        // Business rule (matches your prior page): only Organizer/Instructor/Admin can change hours after creation
        public bool CanEditHours => IsOrganizer || IsInstructor || IsAdmin;
    }

    public sealed record OpportunityRow
    {
        public int Id { get; init; }
        public string Title { get; init; } = "";
        public string? Description { get; init; }
        public DateTime Date { get; init; }
        public int Hours { get; init; }

        public bool IsApproved { get; init; }
        public DateTime? ApprovedAt { get; init; }
        public string? ApprovedByUserId { get; init; }

        public string CreatedByUserId { get; init; } = "";
        public DateTime CreatedAt { get; init; }

        public int ContactId { get; init; }
        public string ContactName { get; init; } = "";
        public string? ContactEmail { get; init; }
        public string? ContactPhone { get; init; }

        public int? AddressId { get; init; }
        public string Street1 { get; init; } = "";
        public string? Street2 { get; init; }
        public string City { get; init; } = "";
        public string State { get; init; } = "";
        public string PostalCode { get; init; } = "";
    }

    public async Task<List<OpportunityRow>> GetAllAsync(CancellationToken ct = default)
    {
        var items = await db.VolunteerOpportunities
            .AsNoTracking()
            .Include(o => o.Contact)
            .ThenInclude(c => c.Address)
            .OrderByDescending(o => o.Date)
            .ToListAsync(ct);

        return items.Select(MapRow).ToList();
    }

    public async Task<(bool ok, string message)> CreateAsync(CreateModel input, ActorContext actor, CancellationToken ct = default)
    {
        if (!actor.CanManage)
            return (false, "Not authorized.");

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

            IsApproved = actor.AutoApprove,
            ApprovedAt = actor.AutoApprove ? now : null,
            ApprovedByUserId = actor.AutoApprove ? actor.UserId : null
        };

        db.VolunteerOpportunities.Add(entity);
        await db.SaveChangesAsync(ct);

        return (true, actor.AutoApprove
            ? "Opportunity created and approved."
            : "Opportunity created (pending approval).");
    }

    public async Task<(bool ok, string message, EditModel? model)> GetEditModelAsync(int id, CancellationToken ct = default)
    {
        var existing = await db.VolunteerOpportunities
            .AsNoTracking()
            .Include(o => o.Contact)
            .ThenInclude(c => c.Address)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

        if (existing is null)
            return (false, "Opportunity not found.", null);

        var m = new EditModel
        {
            Id = existing.Id,

            Title = existing.Title,
            Description = existing.Description,
            Date = existing.Date,
            Hours = existing.Hours,

            IsApproved = existing.IsApproved,

            ContactId = existing.ContactId,
            ContactName = existing.Contact?.Name ?? "",
            ContactEmail = existing.Contact?.Email,
            ContactPhone = existing.Contact?.Phone,

            AddressId = existing.Contact?.AddressId,
            Street1 = existing.Contact?.Address?.Street1 ?? "",
            Street2 = existing.Contact?.Address?.Street2,
            City = existing.Contact?.Address?.City ?? "",
            State = string.IsNullOrWhiteSpace(existing.Contact?.Address?.State) ? "FL" : existing.Contact!.Address!.State,
            PostalCode = existing.Contact?.Address?.PostalCode ?? ""
        };

        return (true, "OK", m);
    }

    public async Task<(bool ok, string message)> UpdateAsync(EditModel input, ActorContext actor, CancellationToken ct = default)
    {
        if (!actor.CanManage)
            return (false, "Not authorized.");

        var existing = await db.VolunteerOpportunities
            .Include(o => o.Contact)
            .ThenInclude(c => c.Address)
            .FirstOrDefaultAsync(o => o.Id == input.Id, ct);

        if (existing is null)
            return (false, "Opportunity not found.");

        // Date rules:
        // - Everyone: cannot edit after the event has passed
        if (existing.Date.Date < DateTime.Today)
            return (false, "This opportunity has already passed and can no longer be edited.");

        // - Organizer-only: cannot edit on the event date (only up to the day before)
        var organizerOnly = actor is { IsOrganizer: true, IsInstructor: false, IsAdmin: false };

        if (organizerOnly && existing.Date.Date == DateTime.Today)
            return (false, "Organizers can only edit opportunities until the day before the event.");


        // Apply opportunity changes
        existing.Title = input.Title.Trim();
        existing.Description = string.IsNullOrWhiteSpace(input.Description) ? null : input.Description.Trim();
        existing.Date = input.Date;

        // Hours rule (matches your earlier UI): only Instructor/Admin can change hours after creation
        if (actor.CanEditHours)
            existing.Hours = input.Hours;

        // Contact changes
        existing.Contact ??= new Contact();
        existing.Contact.Name = input.ContactName.Trim();
        existing.Contact.Email = string.IsNullOrWhiteSpace(input.ContactEmail) ? null : input.ContactEmail.Trim();
        existing.Contact.Phone = string.IsNullOrWhiteSpace(input.ContactPhone) ? null : input.ContactPhone.Trim();

        // Address: optional; if user leaves address essentially blank, drop it
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

        // Approval rules:
        // - Instructor/Admin: auto-approve edits
        // - Organizer: edits force re-approval (pending)
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

        await db.SaveChangesAsync(ct);

        return (true, actor.AutoApprove
            ? "Saved changes (approved)."
            : "Saved changes (pending re-approval).");
    }

    public async Task<(bool ok, string message)> DeleteAsync(int id, ActorContext actor, CancellationToken ct = default)
    {
        if (!actor.CanManage)
            return (false, "Not authorized.");

        var existing = await db.VolunteerOpportunities
            .FirstOrDefaultAsync(o => o.Id == id, ct);

        if (existing is null)
            return (false, "Opportunity not found.");

        // Everyone: cannot delete after the event has passed
        if (existing.Date.Date < DateTime.Today)
            return (false, "This opportunity has already passed and can no longer be deleted.");

        // Organizer-only: cannot delete on the event date (only until day before)
        var organizerOnly = actor is { IsOrganizer: true, IsInstructor: false, IsAdmin: false };

        if (organizerOnly && existing.Date.Date == DateTime.Today)
            return (false, "Organizers can only delete opportunities until the day before the event.");

        // Organizer: request deletion (requires approval)
        if (organizerOnly)
        {
            if (existing.IsDeletionRequested)
                return (true, "Deletion request already pending approval.");

            existing.IsDeletionRequested = true;
            existing.DeletionRequestedAt = DateTime.UtcNow;
            existing.DeletionRequestedByUserId = actor.UserId;

            // Deletion request forces re-approval / pending
            existing.IsApproved = false;
            existing.ApprovedAt = null;
            existing.ApprovedByUserId = null;

            await db.SaveChangesAsync(ct);

            return (true, "Deletion request submitted for approval.");
        }

        // Instructor/Admin: delete immediately
        db.VolunteerOpportunities.Remove(existing);
        await db.SaveChangesAsync(ct);

        return (true, "Opportunity deleted.");
    }


    public async Task<(bool ok, string message)> ApproveAsync(int id, ActorContext actor, CancellationToken ct = default)
    {
        if (!actor.CanApprove)
            return (false, "Not authorized.");

        var existing = await db.VolunteerOpportunities.FirstOrDefaultAsync(o => o.Id == id, ct);
        if (existing is null) return (false, "Opportunity not found.");

        if (existing.IsApproved) return (true, "Already approved.");

        existing.IsApproved = true;
        existing.ApprovedAt = DateTime.UtcNow;
        existing.ApprovedByUserId = actor.UserId;

        await db.SaveChangesAsync(ct);
        return (true, "Opportunity approved.");
    }

    public async Task<(bool ok, string message)> UnapproveAsync(int id, ActorContext actor, CancellationToken ct = default)
    {
        if (!actor.CanApprove)
            return (false, "Not authorized.");

        var existing = await db.VolunteerOpportunities.FirstOrDefaultAsync(o => o.Id == id, ct);
        if (existing is null) return (false, "Opportunity not found.");

        if (!existing.IsApproved) return (true, "Already pending.");

        existing.IsApproved = false;
        existing.ApprovedAt = null;
        existing.ApprovedByUserId = null;

        await db.SaveChangesAsync(ct);
        return (true, "Opportunity set to pending.");
    }

    public async Task<(bool ok, string message)> ApproveDeletionAsync(int id, ActorContext actor, CancellationToken ct = default)
    {
        if (!actor.CanApprove)
            return (false, "Not authorized.");

        var existing = await db.VolunteerOpportunities
            .FirstOrDefaultAsync(o => o.Id == id, ct);

        if (existing is null)
            return (false, "Opportunity not found.");

        if (!existing.IsDeletionRequested)
            return (false, "No deletion request is pending for this opportunity.");

        // Everyone: cannot delete after the event has passed (keep consistent with DeleteAsync rule)
        if (existing.Date.Date < DateTime.Today)
            return (false, "This opportunity has already passed and can no longer be deleted.");

        db.VolunteerOpportunities.Remove(existing);
        await db.SaveChangesAsync(ct);

        return (true, "Deletion approved. Opportunity deleted.");
    }

    public async Task<(bool ok, string message)> RejectDeletionAsync(int id, ActorContext actor, CancellationToken ct = default)
    {
        if (!actor.CanApprove)
            return (false, "Not authorized.");

        var existing = await db.VolunteerOpportunities
            .FirstOrDefaultAsync(o => o.Id == id, ct);

        if (existing is null)
            return (false, "Opportunity not found.");

        if (!existing.IsDeletionRequested)
            return (true, "No deletion request is pending for this opportunity.");

        existing.IsDeletionRequested = false;
        existing.DeletionRequestedAt = null;
        existing.DeletionRequestedByUserId = null;

        // NOTE:
        // We do NOT auto-approve here. It stays pending unless Admin/Instructor explicitly approve it.
        
        await db.SaveChangesAsync(ct);

        return (true, "Deletion request rejected.");
    }


    private static OpportunityRow MapRow(VolunteerOpportunity o)
    {
        var a = o.Contact?.Address;

        return new OpportunityRow
        {
            Id = o.Id,
            Title = o.Title ?? "",
            Description = o.Description,
            Date = o.Date,
            Hours = o.Hours,

            IsApproved = o.IsApproved,
            ApprovedAt = o.ApprovedAt,
            ApprovedByUserId = o.ApprovedByUserId,

            CreatedByUserId = o.CreatedByUserId ?? "",
            CreatedAt = o.CreatedAt,

            ContactId = o.ContactId,
            ContactName = o.Contact?.Name ?? "",
            ContactEmail = o.Contact?.Email,
            ContactPhone = o.Contact?.Phone,

            AddressId = o.Contact?.AddressId,
            Street1 = a?.Street1 ?? "",
            Street2 = a?.Street2,
            City = a?.City ?? "",
            State = a?.State ?? "",
            PostalCode = a?.PostalCode ?? ""
        };
    }

    public async Task<List<VolunteerOpportunity>> GetApprovedAvailableForUserAsync(
        string userId,
        CancellationToken ct = default)
    {
        return await db.VolunteerOpportunities
            .AsNoTracking()
            .Include(o => o.Contact)
            .ThenInclude(c => c.Address)
            .Where(o => o.IsApproved)
            .Where(o => !db.VolunteerSignups.Any(s =>
                s.UserId == userId && s.VolunteerOpportunityId == o.Id))
            .OrderBy(o => o.Date)
            .ToListAsync(ct);
    }

    public async Task<(bool ok, string? error)> SignupAsync(
        int opportunityId,
        string userId,
        CancellationToken ct = default)
    {
        var opp = await db.VolunteerOpportunities
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == opportunityId, ct);

        if (opp is null)
            return (false, "Opportunity not found.");

        if (!opp.IsApproved)
            return (false, "This opportunity is not approved yet.");

        var exists = await db.VolunteerSignups
            .AnyAsync(s => s.VolunteerOpportunityId == opportunityId && s.UserId == userId, ct);

        if (exists)
            return (false, "You are already signed up for this opportunity.");

        db.VolunteerSignups.Add(new VolunteerSignup
        {
            VolunteerOpportunityId = opportunityId,
            UserId = userId,
            SignedUpAt = DateTime.UtcNow,

            // Attendance workflow defaults
            AttendanceSubmitted = false,
            AttendanceSubmittedAt = null,
            AttendanceApproved = false,
            AttendanceApprovedAt = null,
            ApprovedByUserId = null
        });

        await db.SaveChangesAsync(ct);
        return (true, null);
    }

}
