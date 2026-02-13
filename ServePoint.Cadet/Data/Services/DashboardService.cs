using Microsoft.EntityFrameworkCore;
using ServePoint.Cadet.Models.Entities;

namespace ServePoint.Cadet.Data.Services;

public sealed class DashboardService(DbGateway dbGateway)
{
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

    // ---- Time helpers (UTC safe, but compare by DATE only where policy says “event date”) ----
    // NOTE: your VolunteerOpportunity.Date is a DateTime. In Postgres you should treat it as a DATE.
    // The safest approach is to always normalize comparisons to DateOnly (server-agnostic).
    private static DateOnly TodayDateOnlyUtc() => DateOnly.FromDateTime(DateTime.UtcNow);

    private static DateOnly DateOnlyFromEventDate(DateTime eventDate) => DateOnly.FromDateTime(eventDate);

    public Task<DashboardData> LoadAsync(ActorContext actor, CancellationToken ct = default)
        => dbGateway.ExecuteAsync(async db =>
        {
            // My signups (always)
            var mySignups = await db.VolunteerSignups
                .AsNoTracking()
                .Where(s => s.UserId == actor.UserId)
                .Include(s => s.VolunteerOpportunity)
                    .ThenInclude(o => o.Contact)
                        .ThenInclude(c => c.Address)
                .OrderByDescending(s => s.SignedUpAt)
                .ToListAsync(ct);

            var approvedHours = 0;
            var approvedCount = 0;

            if (actor.AccruesHours)
            {
                // Compute from loaded graph (fine)
                approvedHours = mySignups
                    .Where(s => s.AttendanceApproved)
                    .Sum(s => s.VolunteerOpportunity.Hours);

                approvedCount = mySignups.Count(s => s.AttendanceApproved);
            }

            // Upcoming = signed up, not approved yet, and event date is today or later
            var today = TodayDateOnlyUtc();
            var upcomingSignedUp = mySignups.Count(s =>
                !s.AttendanceApproved &&
                DateOnlyFromEventDate(s.VolunteerOpportunity.Date) >= today);

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
        }, ct);

    public Task<(bool ok, string? error)> SubmitAttendanceAsync(string userId, int signupId, CancellationToken ct = default)
        => dbGateway.ExecuteAsync<(bool ok, string? error)>(async db =>
        {
            var signup = await db.VolunteerSignups
                .Include(s => s.VolunteerOpportunity)
                .FirstOrDefaultAsync(s => s.Id == signupId && s.UserId == userId, ct);

            if (signup is null) return (false, "Signup not found.");

            // Business rule (date-based): cannot submit before event date
            var today = TodayDateOnlyUtc();
            var eventDay = DateOnlyFromEventDate(signup.VolunteerOpportunity.Date);

            if (eventDay > today)
                return (false, "You can't mark this completed before the event date.");

            if (signup.AttendanceApproved)
                return (false, "This signup is already approved as completed.");

            if (signup.AttendanceSubmitted)
                return (false, "Completion is already submitted and pending approval.");

            signup.AttendanceSubmitted = true;
            signup.AttendanceSubmittedAt = DateTime.UtcNow;

            await db.SaveChangesAsync(ct);
            return (true, null);
        }, ct);

    public Task<(bool ok, string? error)> ApproveAttendanceAsync(int signupId, ActorContext actor, CancellationToken ct = default)
        => dbGateway.ExecuteAsync<(bool ok, string? error)>(async db =>
        {
            if (!actor.CanApprove) return (false, "Not authorized.");

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
        }, ct);

    public Task<(bool ok, string? error)> RejectAttendanceAsync(int signupId, ActorContext actor, CancellationToken ct = default)
        => dbGateway.ExecuteAsync<(bool ok, string? error)>(async db =>
        {
            if (!actor.CanApprove) return (false, "Not authorized.");

            var signup = await db.VolunteerSignups
                .FirstOrDefaultAsync(s => s.Id == signupId, ct);

            if (signup is null) return (false, "Signup not found.");
            if (!signup.AttendanceSubmitted) return (true, null); // idempotent

            signup.AttendanceSubmitted = false;
            signup.AttendanceSubmittedAt = null;

            signup.AttendanceApproved = false;
            signup.AttendanceApprovedAt = null;
            signup.ApprovedByUserId = null;

            await db.SaveChangesAsync(ct);
            return (true, null);
        }, ct);

    public Task<(bool ok, string? error)> WithdrawAsync(string userId, int signupId, CancellationToken ct = default)
        => dbGateway.ExecuteAsync<(bool ok, string? error)>(async db =>
        {
            var signup = await db.VolunteerSignups
                .Include(s => s.VolunteerOpportunity)
                .FirstOrDefaultAsync(s => s.Id == signupId && s.UserId == userId, ct);

            if (signup is null)
                return (false, "Signup not found.");

            // Business rule (date-based): cannot withdraw after event date has passed
            var today = TodayDateOnlyUtc();
            var eventDay = DateOnlyFromEventDate(signup.VolunteerOpportunity.Date);

            if (eventDay < today)
                return (false, "This event has already passed. You can no longer withdraw.");

            if (signup.AttendanceSubmitted || signup.AttendanceApproved)
                return (false, "This signup is already submitted for completion and cannot be withdrawn.");

            db.VolunteerSignups.Remove(signup);
            await db.SaveChangesAsync(ct);

            return (true, null);
        }, ct);
}
