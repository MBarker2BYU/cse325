using Microsoft.EntityFrameworkCore;
using ServePoint.Cadet.Data;
using ServePoint.Cadet.Models.Entities;

namespace ServePoint.Cadet.Services;

public class OpportunityService(ServePointCadetContext db)
{
    public async Task<List<VolunteerOpportunity>> GetAllAsync(CancellationToken ct = default)
    {
        return await db.VolunteerOpportunities
            .AsNoTracking()
            .OrderByDescending(o => o.Date)
            .ThenBy(o => o.Title)
            .ToListAsync(ct);
    }

    public async Task<int> CreateAsync(VolunteerOpportunity opportunity, CancellationToken ct = default)
    {
        db.VolunteerOpportunities.Add(opportunity);
        await db.SaveChangesAsync(ct);
        return opportunity.Id;
    }

    public async Task<bool> UpdateAsync(VolunteerOpportunity updated, CancellationToken ct = default)
    {
        var existing = await db.VolunteerOpportunities
            .FirstOrDefaultAsync(o => o.Id == updated.Id, ct);

        if (existing is null) return false;

        // Allow editing core fields (you can lock Hours later if desired)
        existing.Title = updated.Title;
        existing.Description = updated.Description;
        existing.Date = updated.Date;
        existing.Location = updated.Location;
        existing.Hours = updated.Hours;

        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var existing = await db.VolunteerOpportunities.FirstOrDefaultAsync(o => o.Id == id, ct);
        if (existing is null) return false;

        // If you later add cascade rules/signups, we’ll handle it properly here
        db.VolunteerOpportunities.Remove(existing);
        await db.SaveChangesAsync(ct);
        return true;
    }
}