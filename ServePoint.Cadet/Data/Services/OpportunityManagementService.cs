using Microsoft.EntityFrameworkCore;
using ServePoint.Cadet.Models.Entities;
using ServePoint.Cadet.Models.Opportunities;

namespace ServePoint.Cadet.Data.Services;

public sealed class OpportunityManagementService(DbGateway dbg)
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

        // Business rule: only Organizer/Instructor/Admin can change hours after creation
        public bool CanEditHours => IsOrganizer || IsInstructor || IsAdmin;
    }

    public sealed record OpportunityRow
    {
        public int Id { get; init; }
        public string Title { get; init; } = "";
        public string? Description { get; init; }

        public DateTime Date { get; init; }
        public TimeOnly StartTime { get; init; }
        public TimeOnly EndTime { get; init; }

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

    // ===== Date/Time helpers =====
    // PostgreSQL "timestamptz" requires UTC DateTime values in Npgsql.
    // Treat the event "Date" as a date-only concept stored as UTC midnight.
    private static DateTime NormalizeEventDateUtc(DateTime date)
        => DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);

    private static DateTime EndDateTime(DateTime dateUtcMidnight, TimeOnly start, TimeOnly end)
    {
        var baseDate = NormalizeEventDateUtc(dateUtcMidnight);
        var endDt = baseDate.Add(end.ToTimeSpan());

        // Overnight: ends next day
        if (end <= start)
            endDt = endDt.AddDays(1);

        return DateTime.SpecifyKind(endDt, DateTimeKind.Utc);
    }

    private static bool HasEnded(VolunteerOpportunity o)
        => EndDateTime(o.Date, o.StartTime, o.EndTime) <= DateTime.UtcNow;

    private static bool HasEnded(OpportunityRow row)
        => EndDateTime(row.Date, row.StartTime, row.EndTime) <= DateTime.UtcNow;

    private static bool ValidateTimes(TimeOnly start, TimeOnly end, out string? error)
    {
        error = null;
        return true;
    }

    public Task<List<OpportunityRow>> GetAllAsync(CancellationToken ct = default)
        => dbg.ExecuteAsync(async db =>
        {
            var items = await db.VolunteerOpportunities
                .AsNoTracking()
                .Include(o => o.Contact)
                .ThenInclude(c => c.Address)
                .OrderByDescending(o => o.Date)
                .ToListAsync(ct);

            return items.Select(MapRow).ToList();
        }, ct);

    public async Task<(bool ok, string message)> CreateAsync(CreateModel input, ActorContext actor, CancellationToken ct = default)
    {
        if (!actor.CanManage)
            return (false, "Not authorized.");

        // Normalize to UTC midnight to satisfy Npgsql timestamp tz rules
        input.Date = NormalizeEventDateUtc(input.Date);

        if (!ValidateTimes(input.StartTime, input.EndTime, out var timeError))
            return (false, timeError!);

        var now = DateTime.UtcNow;

        var ok = await dbg.ExecuteAsync(async db =>
        {
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

                Date = input.Date, // already normalized UTC midnight
                StartTime = input.StartTime,
                EndTime = input.EndTime,

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
            return true;
        }, ct);

        return ok
            ? (true, actor.AutoApprove ? "Opportunity created and approved." : "Opportunity created (pending approval).")
            : (false, "Failed to create opportunity.");
    }

    public Task<(bool ok, string message, EditModel? model)> GetEditModelAsync(int id, CancellationToken ct = default)
        => dbg.ExecuteAsync<(bool ok, string message, EditModel? model)>(async db =>
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
                StartTime = existing.StartTime,
                EndTime = existing.EndTime,

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
        }, ct);

    public async Task<(bool ok, string message)> UpdateAsync(EditModel input, ActorContext actor, CancellationToken ct = default)
    {
        if (!actor.CanManage)
            return (false, "Not authorized.");

        // Normalize to UTC midnight to satisfy Npgsql timestamp tz rules
        input.Date = NormalizeEventDateUtc(input.Date);

        if (!ValidateTimes(input.StartTime, input.EndTime, out var timeError))
            return (false, timeError!);

        return await dbg.ExecuteAsync(async db =>
        {
            var existing = await db.VolunteerOpportunities
                .Include(o => o.Contact)
                .ThenInclude(c => c.Address)
                .FirstOrDefaultAsync(o => o.Id == input.Id, ct);

            if (existing is null)
                return (false, "Opportunity not found.");

            if (HasEnded(existing))
                return (false, "This opportunity has already ended and can no longer be edited.");

            var organizerOnly = actor is { IsOrganizer: true, IsInstructor: false, IsAdmin: false };
            if (organizerOnly && existing.Date.Date == DateTime.UtcNow.Date)
                return (false, "Organizers can only edit opportunities until the day before the event.");

            existing.Title = input.Title.Trim();
            existing.Description = string.IsNullOrWhiteSpace(input.Description) ? null : input.Description.Trim();

            existing.Date = input.Date; // already normalized UTC midnight
            existing.StartTime = input.StartTime;
            existing.EndTime = input.EndTime;

            if (actor.CanEditHours)
                existing.Hours = input.Hours;

            existing.Contact ??= new Contact();
            existing.Contact.Name = input.ContactName.Trim();
            existing.Contact.Email = string.IsNullOrWhiteSpace(input.ContactEmail) ? null : input.ContactEmail.Trim();
            existing.Contact.Phone = string.IsNullOrWhiteSpace(input.ContactPhone) ? null : input.ContactPhone.Trim();

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
        }, ct);
    }

    public Task<(bool ok, string message)> DeleteAsync(int id, ActorContext actor, CancellationToken ct = default)
        => dbg.ExecuteAsync<(bool ok, string message)>(async db =>
        {
            if (!actor.CanManage)
                return (false, "Not authorized.");

            var existing = await db.VolunteerOpportunities
                .FirstOrDefaultAsync(o => o.Id == id, ct);

            if (existing is null)
                return (false, "Opportunity not found.");

            if (HasEnded(existing))
                return (false, "This opportunity has already ended and can no longer be deleted.");

            var organizerOnly = actor is { IsOrganizer: true, IsInstructor: false, IsAdmin: false };
            if (organizerOnly && existing.Date.Date == DateTime.UtcNow.Date)
                return (false, "Organizers can only delete opportunities until the day before the event.");

            if (organizerOnly)
            {
                if (existing.IsDeletionRequested)
                    return (true, "Deletion request already pending approval.");

                existing.IsDeletionRequested = true;
                existing.DeletionRequestedAt = DateTime.UtcNow;
                existing.DeletionRequestedByUserId = actor.UserId;

                existing.IsApproved = false;
                existing.ApprovedAt = null;
                existing.ApprovedByUserId = null;

                await db.SaveChangesAsync(ct);
                return (true, "Deletion request submitted for approval.");
            }

            db.VolunteerOpportunities.Remove(existing);
            await db.SaveChangesAsync(ct);
            return (true, "Opportunity deleted.");
        });

    public Task<(bool ok, string message)> ApproveAsync(int id, ActorContext actor, CancellationToken ct = default)
        => dbg.ExecuteAsync<(bool ok, string message)>(async db =>
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
        });

    public Task<(bool ok, string message)> UnapproveAsync(int id, ActorContext actor, CancellationToken ct = default)
        => dbg.ExecuteAsync<(bool ok, string message)>(async db =>
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
        });

    public Task<(bool ok, string message)> ApproveDeletionAsync(int id, ActorContext actor, CancellationToken ct = default)
        => dbg.ExecuteAsync<(bool ok, string message)>(async db =>
        {
            if (!actor.CanApprove)
                return (false, "Not authorized.");

            var existing = await db.VolunteerOpportunities
                .FirstOrDefaultAsync(o => o.Id == id, ct);

            if (existing is null)
                return (false, "Opportunity not found.");

            if (!existing.IsDeletionRequested)
                return (false, "No deletion request is pending for this opportunity.");

            if (HasEnded(existing))
                return (false, "This opportunity has already ended and can no longer be deleted.");

            db.VolunteerOpportunities.Remove(existing);
            await db.SaveChangesAsync(ct);

            return (true, "Deletion approved. Opportunity deleted.");
        });

    public Task<(bool ok, string message)> RejectDeletionAsync(int id, ActorContext actor, CancellationToken ct = default)
        => dbg.ExecuteAsync<(bool ok, string message)>(async db =>
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

            await db.SaveChangesAsync(ct);

            return (true, "Deletion request rejected.");
        });

    private static OpportunityRow MapRow(VolunteerOpportunity o)
    {
        var a = o.Contact?.Address;

        return new OpportunityRow
        {
            Id = o.Id,
            Title = o.Title ?? "",
            Description = o.Description,

            Date = o.Date,
            StartTime = o.StartTime,
            EndTime = o.EndTime,

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

    public Task<List<VolunteerOpportunity>> GetApprovedAvailableForUserAsync(
        string userId,
        CancellationToken ct = default)
        => dbg.ExecuteAsync(async db =>
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
        });

    public Task<(bool ok, string? error)> SignupAsync(
        int opportunityId,
        string userId,
        CancellationToken ct = default)
        => dbg.ExecuteAsync<(bool ok, string? error)>(async db =>
        {
            var opp = await db.VolunteerOpportunities
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == opportunityId, ct);

            if (opp is null)
                return (false, "Opportunity not found.");

            if (!opp.IsApproved)
                return (false, "This opportunity is not approved yet.");

            if (HasEnded(opp))
                return (false, "This opportunity has already ended.");

            var exists = await db.VolunteerSignups
                .AnyAsync(s => s.VolunteerOpportunityId == opportunityId && s.UserId == userId, ct);

            if (exists)
                return (false, "You are already signed up for this opportunity.");

            db.VolunteerSignups.Add(new VolunteerSignup
            {
                VolunteerOpportunityId = opportunityId,
                UserId = userId,
                SignedUpAt = DateTime.UtcNow,

                AttendanceSubmitted = false,
                AttendanceSubmittedAt = null,
                AttendanceApproved = false,
                AttendanceApprovedAt = null,
                ApprovedByUserId = null
            });

            await db.SaveChangesAsync(ct);
            return (true, null);
        });
}
