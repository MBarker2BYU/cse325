using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ServePoint.Cadet.Reports.Services;

namespace ServePoint.Cadet.Reports;

public static class VolunteerHoursToPDF
{
    public static byte[] Build(
        VolunteerHoursReportService.VolunteerHoursReportResult report,
        DateTime? from,
        DateTime? to)
    {
        return Document.Create(doc =>
        {
            doc.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(36);

                page.Content().Column(col =>
                {
                    col.Spacing(10);

                    col.Item().Text("SERVEPOINT VOLUNTEER HOURS REPORT")
                        .FontSize(16).Bold();

                    col.Item().Text($"Cadet: {report.HeaderName} ({report.HeaderEmail})");
                    col.Item().Text($"Range: {from:yyyy-MM-dd} – {to:yyyy-MM-dd}");
                    col.Item().Text($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}");

                    col.Item().LineHorizontal(1);

                    col.Item().Table(t =>
                    {
                        t.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(90);
                            c.RelativeColumn();
                            c.ConstantColumn(60);
                            c.ConstantColumn(90);
                        });

                        static IContainer Cell(IContainer x) => x.Border(1).Padding(4);

                        Cell(t.Cell()).Text("Date").Bold();
                        Cell(t.Cell()).Text("Opportunity").Bold();
                        Cell(t.Cell()).AlignRight().Text("Hours").Bold();
                        Cell(t.Cell()).Text("Status").Bold();

                        foreach (var r in report.Rows)
                        {
                            Cell(t.Cell()).Text(r.Date.ToString("yyyy-MM-dd"));
                            Cell(t.Cell()).Text(r.Title);
                            Cell(t.Cell()).AlignRight().Text(r.Hours.ToString());
                            Cell(t.Cell()).Text(r.Status);
                        }

                        Cell(t.Cell().ColumnSpan(2)).AlignRight().Text("Approved Total").Bold();
                        Cell(t.Cell()).AlignRight().Text(report.ApprovedTotalHours.ToString()).Bold();
                        Cell(t.Cell()).Text("");

                        Cell(t.Cell().ColumnSpan(2)).AlignRight().Text("Pending Total").Bold();
                        Cell(t.Cell()).AlignRight().Text(report.PendingTotalHours.ToString()).Bold();
                        Cell(t.Cell()).Text("");
                    });
                });
            });
        }).GeneratePdf();
    }
}
