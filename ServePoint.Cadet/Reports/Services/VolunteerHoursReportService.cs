using Microsoft.EntityFrameworkCore;
using ServePoint.Cadet.Auth;
using ServePoint.Cadet.Data;

namespace ServePoint.Cadet.Reports.Services;

public sealed class VolunteerHoursReportService(DbGateway dbg)
{
    // Instructor/Admin can pick any cadet (Users/Organizers).
    public Task<List<CadetPick>> GetCadetPickerAsync(CancellationToken ct = default)
        => dbg.ExecuteAsync(async db =>
        {
            var cadetRoleNames = new[] { Roles.User, Roles.Organizer };

            var cadets =
                from u in db.Users
                join ur in db.UserRoles on u.Id equals ur.UserId
                join r in db.Roles on ur.RoleId equals r.Id
                where cadetRoleNames.Contains(r.Name!)
                orderby u.UserName
                select new { u.Id, u.UserName, u.Email };

            var list = await cadets.Distinct().ToListAsync(ct);

            return list
                .Select(x => new CadetPick(x.Id, $"{x.UserName} ({x.Email})"))
                .ToList();
        }, ct);

    public Task<VolunteerHoursReportResult> GetReportAsync(
        string requesterId,
        string targetUserId,
        DateTime? from,
        DateTime? to,
        CancellationToken ct = default)
        => dbg.ExecuteAsync(async db =>
        {
            // ---- Authorization WITHOUT UserManager (avoids DbContext concurrency issues) ----
            var requesterRoleNames =
                from ur in db.UserRoles
                join r in db.Roles on ur.RoleId equals r.Id
                where ur.UserId == requesterId
                select r.Name!;

            var roles = await requesterRoleNames.ToListAsync(ct);

            var canViewOthers = roles.Contains(Roles.Instructor) || roles.Contains(Roles.Admin);

            if (!canViewOthers && requesterId != targetUserId)
                throw new UnauthorizedAccessException("Not allowed to view other users' reports.");

            // ---- Header ----
            var target = await db.Users
                .Where(u => u.Id == targetUserId)
                .Select(u => new { u.UserName, u.Email })
                .SingleAsync(ct);

            // ---- Base query: signups for this user + opportunity info ----
            var q =
                from s in db.VolunteerSignups.AsNoTracking()
                join o in db.VolunteerOpportunities.AsNoTracking()
                    on s.VolunteerOpportunityId equals o.Id
                where s.UserId == targetUserId
                select new { s, o };

            // Normalize filters to UTC (Postgres timestamptz expects UTC with Npgsql)
            static DateTime AsUtcDate(DateTime dt)
                => DateTime.SpecifyKind(dt.Date, DateTimeKind.Utc);

            if (from.HasValue)
            {
                var f = AsUtcDate(from.Value);
                q = q.Where(x => x.o.Date >= f);
            }

            if (to.HasValue)
            {
                // inclusive end-of-day
                var t = DateTime.SpecifyKind(to.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
                q = q.Where(x => x.o.Date <= t);
            }

            // ---- Rows ----
            var rows = await q
                .OrderByDescending(x => x.o.Date)
                .Select(x => new VolunteerHoursRow(
                    Date: x.o.Date,
                    Title: x.o.Title,
                    Hours: x.o.Hours,
                    Status: x.s.AttendanceApproved
                        ? "Approved"
                        : x.s.AttendanceSubmitted ? "Pending" : "Signed Up"
                ))
                .ToListAsync(ct);

            // ---- Totals ----
            var approvedTotal = rows.Where(r => r.Status == "Approved").Sum(r => r.Hours);
            var pendingTotal = rows.Where(r => r.Status == "Pending").Sum(r => r.Hours);

            return new VolunteerHoursReportResult(
                HeaderName: target.UserName ?? "(unknown)",
                HeaderEmail: target.Email ?? "",
                ApprovedTotalHours: approvedTotal,
                PendingTotalHours: pendingTotal,
                Rows: rows
            );
        }, ct);

    public byte[] BuildCsvBytes(
        VolunteerHoursReportResult report,
        DateTime? from,
        DateTime? to)
    {
        static string Esc(string s)
        {
            s ??= "";
            if (s.Contains('"') || s.Contains(',') || s.Contains('\n') || s.Contains('\r'))
                return $"\"{s.Replace("\"", "\"\"")}\"";
            return s;
        }

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("ServePoint Volunteer Hours Report");
        sb.AppendLine($"Cadet,{Esc(report.HeaderName)}");
        sb.AppendLine($"Email,{Esc(report.HeaderEmail)}");
        sb.AppendLine($"From,{from:yyyy-MM-dd}");
        sb.AppendLine($"To,{to:yyyy-MM-dd}");
        sb.AppendLine();
        sb.AppendLine("Date,Opportunity,Hours,Status");

        foreach (var r in report.Rows)
            sb.AppendLine($"{r.Date:yyyy-MM-dd},{Esc(r.Title)},{r.Hours},{Esc(r.Status)}");

        sb.AppendLine();
        sb.AppendLine($",Approved Total,{report.ApprovedTotalHours},");
        sb.AppendLine($",Pending Total,{report.PendingTotalHours},");
        return System.Text.Encoding.UTF8.GetBytes(sb.ToString());
    }

    public record VolunteerHoursRow(DateTime Date, string Title, int Hours, string Status);
    public record CadetPick(string Id, string Display);

    public record VolunteerHoursReportResult(
        string HeaderName,
        string HeaderEmail,
        int ApprovedTotalHours,
        int PendingTotalHours,
        List<VolunteerHoursRow> Rows);
}
