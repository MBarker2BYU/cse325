using Microsoft.EntityFrameworkCore;
using ServePoint.Cadet.Data;
using ServePoint.Cadet.Models.Entities;

namespace ServePoint.Cadet.Services;

public class OpportunityService(ServePointCadetContext database)
{
    public sealed record OpportunityEditPolicy(bool CanEditHours, bool CanEditAfterApproval);

    public async Task<List<VolunteerOpportunity>> GetAllAsync()
    {
        return await database.VolunteerOpportunities
            .AsNoTracking()
            .Include(o => o.Contact)
            .ThenInclude(c => c.Address)
            .OrderByDescending(o => o.Date)
            .ToListAsync();
    }


    public async Task<int> CreateAsync(VolunteerOpportunity opportunity, CancellationToken ct = default)
    {
        database.VolunteerOpportunities.Add(opportunity);
        await database.SaveChangesAsync(ct);
        return opportunity.Id;
    }

    public async Task<(bool ok, string? error)> UpdateAsync(
        VolunteerOpportunity updated,
        Contact updatedContact,
        Address? updatedAddress,
        OpportunityEditPolicy policy,
        CancellationToken ct = default)
    {
        var existing = await database.VolunteerOpportunities
            .Include(o => o.Contact)
            .ThenInclude(c => c.Address)
            .FirstOrDefaultAsync(o => o.Id == updated.Id, ct);

        if (existing is null) return (false, "Opportunity not found.");

        // Rule: Organizers cannot edit anything after approval
        if (existing.IsApproved && !policy.CanEditAfterApproval)
            return (false, "This opportunity has been approved and can no longer be edited.");

        // Opportunity fields (allowed for organizer pre-approval; allowed always for admin/instructor)
        existing.Title = updated.Title;
        existing.Description = updated.Description;
        existing.Date = updated.Date;

        // Rule: Only Admin/Instructor can change Hours after creation
        if (policy.CanEditHours)
            existing.Hours = updated.Hours;

        // Contact
        existing.Contact.Name = updatedContact.Name;
        existing.Contact.Email = updatedContact.Email;
        existing.Contact.Phone = updatedContact.Phone;

        // Address (optional)
        if (updatedAddress is null)
        {
            existing.Contact.AddressId = null;
            existing.Contact.Address = null;
        }
        else
        {
            existing.Contact.Address ??= new Address();

            existing.Contact.Address.Street1 = updatedAddress.Street1;
            existing.Contact.Address.Street2 = updatedAddress.Street2;
            existing.Contact.Address.City = updatedAddress.City;
            existing.Contact.Address.State = updatedAddress.State;
            existing.Contact.Address.PostalCode = updatedAddress.PostalCode;
        }

        await database.SaveChangesAsync(ct);
        return (true, null);
    }


    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var existing = await database.VolunteerOpportunities.FirstOrDefaultAsync(o => o.Id == id, ct);
        if (existing is null) return false;

        // If you later add cascade rules/signups, we’ll handle it properly here
        database.VolunteerOpportunities.Remove(existing);
        await database.SaveChangesAsync(ct);
        return true;
    }

    public async Task<(bool ok, string? error)> ApproveAsync(int id, string approverUserId, CancellationToken ct = default)
    {
        var existing = await database.VolunteerOpportunities.FirstOrDefaultAsync(o => o.Id == id, ct);
        if (existing is null) return (false, "Opportunity not found.");

        if (existing.IsApproved) return (true, null); // idempotent

        existing.IsApproved = true;
        existing.ApprovedAt = DateTime.UtcNow;
        existing.ApprovedByUserId = approverUserId;

        await database.SaveChangesAsync(ct);
        return (true, null);
    }

    public async Task<(bool ok, string? error)> UnapproveAsync(int id, string approverUserId, CancellationToken ct = default)
    {
        var existing = await database.VolunteerOpportunities.FirstOrDefaultAsync(o => o.Id == id, ct);
        if (existing is null) return (false, "Opportunity not found.");

        if (!existing.IsApproved) return (true, null);

        existing.IsApproved = false;
        existing.ApprovedAt = null;
        existing.ApprovedByUserId = null;

        await database.SaveChangesAsync(ct);
        return (true, null);
    }

    public async Task<List<VolunteerOpportunity>> GetApprovedAvailableForUserAsync(
        string userId,
        CancellationToken ct = default)
    {
        return await database.VolunteerOpportunities
            .AsNoTracking()
            .Include(o => o.Contact)
            .ThenInclude(c => c.Address)
            .Where(o => o.IsApproved)
            .Where(o => !database.VolunteerSignups.Any(s =>
                s.UserId == userId && s.VolunteerOpportunityId == o.Id))
            .OrderBy(o => o.Date)
            .ToListAsync(ct);
    }


    public async Task<(bool ok, string? error)> SignupAsync(int opportunityId, string userId, CancellationToken ct = default)
    {
        // Must exist and be approved
        var opp = await database.VolunteerOpportunities
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == opportunityId, ct);

        if (opp is null) return (false, "Opportunity not found.");
        if (!opp.IsApproved) return (false, "This opportunity is not approved yet.");

        // Prevent duplicate signup
        var exists = await database.VolunteerSignups
            .AnyAsync(s => s.VolunteerOpportunityId == opportunityId && s.UserId == userId, ct);

        if (exists) return (false, "You are already signed up for this opportunity.");

        // Create signup
        database.VolunteerSignups.Add(new VolunteerSignup
        {
            VolunteerOpportunityId = opportunityId,
            UserId = userId,
            SignedUpAt = DateTime.UtcNow
        });

        await database.SaveChangesAsync(ct);
        return (true, null);
    }

    public async Task<(bool ok, string? error)> WithdrawAsync(int signupId, string userId, CancellationToken ct = default)
    {
        var signup = await database.VolunteerSignups
            .Include(s => s.VolunteerOpportunity)
            .FirstOrDefaultAsync(s => s.Id == signupId && s.UserId == userId, ct);

        if (signup is null) return (false, "Signup not found.");

        // Read-only after event date
        if (signup.VolunteerOpportunity.Date.Date < DateTime.Today)
            return (false, "This event has already passed. You can no longer withdraw.");

        // Optional: block withdrawing if they've already marked completed
        if (signup.IsCompleted)
            return (false, "This signup is marked completed and cannot be withdrawn.");

        database.VolunteerSignups.Remove(signup);
        await database.SaveChangesAsync(ct);
        return (true, null);
    }


}