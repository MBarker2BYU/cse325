using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using ServePoint.Cadet.Data;
using ServePoint.Cadet.Models.Entities;

namespace ServePoint.Cadet.Services;

public sealed class DashboardService(ServePointCadetContext db)
{
    public sealed record DashboardData(
        int TotalCompletedHours,
        int TotalCompletedCount,
        int UpcomingSignedUpCount,
        List<VolunteerSignup> MySignups
    );

    public async Task<DashboardData> GetMyDashboardAsync(string userId, CancellationToken ct = default)
    {
        // Pull signups + opportunity + contact/address in one go
        var signups = await db.VolunteerSignups
            .AsNoTracking()
            .Where(s => s.UserId == userId)
            .Include(s => s.VolunteerOpportunity)
            .ThenInclude(o => o.Contact)
            .ThenInclude(c => c.Address)
            .OrderByDescending(s => s.SignedUpAt)
            .ToListAsync(ct);

        // Completed hours based on CURRENT schema (IsCompleted)
        var completedHours = signups
            .Where(s => s.IsCompleted)
            .Sum(s => s.VolunteerOpportunity.Hours);

        var completedCount = signups.Count(s => s.IsCompleted);

        var upcomingSignedUp = signups.Count(s =>
            !s.IsCompleted &&
            s.VolunteerOpportunity.Date.Date >= DateTime.Today);

        return new DashboardData(
            TotalCompletedHours: completedHours,
            TotalCompletedCount: completedCount,
            UpcomingSignedUpCount: upcomingSignedUp,
            MySignups: signups
        );
    }

    public async Task<(bool ok, string? error)> WithdrawAsync(string userId, int signupId, CancellationToken ct = default)
    {
        var signup = await db.VolunteerSignups
            .Include(s => s.VolunteerOpportunity)
            .FirstOrDefaultAsync(s => s.Id == signupId && s.UserId == userId, ct);

        if (signup is null)
            return (false, "Signup not found.");

        // Rule: can only withdraw before the opportunity date has passed
        if (signup.VolunteerOpportunity.Date.Date < DateTime.Today)
            return (false, "This event has already passed. You can no longer withdraw.");

        // Optional rule (recommended): can't withdraw if already completed
        if (signup.IsCompleted)
            return (false, "This signup is marked completed and cannot be withdrawn.");

        db.VolunteerSignups.Remove(signup);
        await db.SaveChangesAsync(ct);

        return (true, null);
    }

}