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
            //var q =
            //    from s in db.VolunteerSignups.AsNoTracking()
            //    join o in db.VolunteerOpportunities.AsNoTracking()
            //        on s.VolunteerOpportunityId equals o.Id
            //    where s.UserId == targetUserId
            //    select new { s, o };

            // ---- Base query: signups for this user + opportunity info ----
            // Rule: do NOT include deleted opportunities UNLESS attendance was approved.
            var q =
                from s in db.VolunteerSignups.AsNoTracking()
                join o in db.VolunteerOpportunities.AsNoTracking()
                    on s.VolunteerOpportunityId equals o.Id
                where s.UserId == targetUserId
                      && (o.DeletedAt == null || s.AttendanceApproved)
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
                    Title: x.o.DeletedAt == null ? x.o.Title : $"{x.o.Title} (Deleted)",
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

        const string nl = "\r\n";
        var sb = new System.Text.StringBuilder();

        sb.Append("ServePoint Volunteer Hours Report").Append(nl);
        sb.Append("Cadet,").Append(Esc(report.HeaderName)).Append(nl);
        sb.Append("Email,").Append(Esc(report.HeaderEmail)).Append(nl);
        sb.Append("From,").Append(from?.ToString("yyyy-MM-dd") ?? "").Append(nl);
        sb.Append("To,").Append(to?.ToString("yyyy-MM-dd") ?? "").Append(nl);
        sb.Append(nl);

        sb.Append("Date,Opportunity,Hours,Status").Append(nl);

        foreach (var r in report.Rows)
            sb.Append(r.Date.ToString("yyyy-MM-dd")).Append(',')
                .Append(Esc(r.Title)).Append(',')
                .Append(r.Hours).Append(',')
                .Append(Esc(r.Status)).Append(nl);

        sb.Append(nl);
        sb.Append(",Approved Total,").Append(report.ApprovedTotalHours).Append(',').Append(nl);
        sb.Append(",Pending Total,").Append(report.PendingTotalHours).Append(',').Append(nl);

        // UTF-8 BOM helps Excel
        var withBom = "\uFEFF" + sb.ToString();
        return System.Text.Encoding.UTF8.GetBytes(withBom);
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
