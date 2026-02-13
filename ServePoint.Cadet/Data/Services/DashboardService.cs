using Microsoft.EntityFrameworkCore;
using ServePoint.Cadet.Data;
using ServePoint.Cadet.Models.Entities;

namespace ServePoint.Cadet.Data.Services;

public sealed class DashboardService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public DashboardService(IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public sealed record ActorContext(
        string UserId,
        bool IsUser,
        bool IsOrganizer,
        bool IsInstructor,
        bool IsAdmin)
    {
        public bool CanApprove => IsInstructor || IsAdmin;
        public bool AccruesHours => IsUser || IsOrganizer; // Instructor/Admin do NOT accrue
    }

    public sealed record DashboardData(
        ActorContext Actor,
        int TotalApprovedHours,
        int TotalApprovedCount,
        int UpcomingSignedUpCount,
        List<VolunteerSignup> MySignups,
        List<VolunteerOpportunity> PendingOpportunities,
        List<VolunteerSignup> PendingAttendance
    );

    public async Task<DashboardData> LoadAsync(ActorContext actor, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        // My signups (always)
        var mySignups = await db.VolunteerSignups
            .AsNoTracking()
            .Where(s => s.UserId == actor.UserId)
            .Include(s => s.VolunteerOpportunity)
                .ThenInclude(o => o.Contact)
                    .ThenInclude(c => c.Address)
            .OrderByDescending(s => s.SignedUpAt)
            .ToListAsync(ct);

        // Approved attendance totals (only if the role accrues hours)
        var approvedHours = 0;
        var approvedCount = 0;

        if (actor.AccruesHours)
        {
            approvedHours = mySignups
                .Where(s => s.AttendanceApproved)
                .Sum(s => s.VolunteerOpportunity.Hours);

            approvedCount = mySignups.Count(s => s.AttendanceApproved);
        }

        var upcomingSignedUp = mySignups.Count(s =>
            !s.AttendanceApproved &&
            s.VolunteerOpportunity.Date.Date >= DateTime.Today);

        // Approval queues (Instructor/Admin)
        var pendingOpps = new List<VolunteerOpportunity>();
        var pendingAttendance = new List<VolunteerSignup>();

        if (actor.CanApprove)
        {
            pendingOpps = await db.VolunteerOpportunities
                .AsNoTracking()
                .Include(o => o.Contact)
                    .ThenInclude(c => c.Address)
                .Where(o => !o.IsApproved)
                .OrderBy(o => o.Date)
                .ToListAsync(ct);

            pendingAttendance = await db.VolunteerSignups
                .AsNoTracking()
                .Include(s => s.VolunteerOpportunity)
                    .ThenInclude(o => o.Contact)
                .Where(s => s.AttendanceSubmitted && !s.AttendanceApproved)
                .OrderBy(s => s.SignedUpAt)
                .ToListAsync(ct);
        }

        return new DashboardData(
            Actor: actor,
            TotalApprovedHours: approvedHours,
            TotalApprovedCount: approvedCount,
            UpcomingSignedUpCount: upcomingSignedUp,
            MySignups: mySignups,
            PendingOpportunities: pendingOpps,
            PendingAttendance: pendingAttendance
        );
    }

    public async Task<(bool ok, string? error)> SubmitAttendanceAsync(string userId, int signupId, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var signup = await db.VolunteerSignups
            .Include(s => s.VolunteerOpportunity)
            .FirstOrDefaultAsync(s => s.Id == signupId && s.UserId == userId, ct);

        if (signup is null) return (false, "Signup not found.");

        if (signup.VolunteerOpportunity.Date.Date > DateTime.Today)
            return (false, "You can't mark this completed before the event date.");

        if (signup.AttendanceApproved)
            return (false, "This signup is already approved as completed.");

        if (signup.AttendanceSubmitted)
            return (false, "Completion is already submitted and pending approval.");

        signup.AttendanceSubmitted = true;
        signup.AttendanceSubmittedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        return (true, null);
    }

    public async Task<(bool ok, string? error)> ApproveAttendanceAsync(int signupId, ActorContext actor, CancellationToken ct = default)
    {
        if (!actor.CanApprove) return (false, "Not authorized.");

        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var signup = await db.VolunteerSignups
            .FirstOrDefaultAsync(s => s.Id == signupId, ct);

        if (signup is null) return (false, "Signup not found.");
        if (!signup.AttendanceSubmitted) return (false, "Attendance was not submitted.");
        if (signup.AttendanceApproved) return (true, null); // idempotent

        signup.AttendanceApproved = true;
        signup.AttendanceApprovedAt = DateTime.UtcNow;
        signup.ApprovedByUserId = actor.UserId;

        await db.SaveChangesAsync(ct);
        return (true, null);
    }

    public async Task<(bool ok, string? error)> RejectAttendanceAsync(int signupId, ActorContext actor, CancellationToken ct = default)
    {
        if (!actor.CanApprove) return (false, "Not authorized.");

        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var signup = await db.VolunteerSignups
            .FirstOrDefaultAsync(s => s.Id == signupId, ct);

        if (signup is null) return (false, "Signup not found.");
        if (!signup.AttendanceSubmitted) return (true, null);

        signup.AttendanceSubmitted = false;
        signup.AttendanceSubmittedAt = null;

        signup.AttendanceApproved = false;
        signup.AttendanceApprovedAt = null;
        signup.ApprovedByUserId = null;

        await db.SaveChangesAsync(ct);
        return (true, null);
    }

    public async Task<(bool ok, string? error)> WithdrawAsync(string userId, int signupId, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var signup = await db.VolunteerSignups
            .Include(s => s.VolunteerOpportunity)
            .FirstOrDefaultAsync(s => s.Id == signupId && s.UserId == userId, ct);

        if (signup is null)
            return (false, "Signup not found.");

        if (signup.VolunteerOpportunity.Date.Date < DateTime.Today)
            return (false, "This event has already passed. You can no longer withdraw.");

        if (signup.AttendanceSubmitted || signup.AttendanceApproved)
            return (false, "This signup is already submitted for completion and cannot be withdrawn.");

        db.VolunteerSignups.Remove(signup);
        await db.SaveChangesAsync(ct);

        return (true, null);
    }
}
