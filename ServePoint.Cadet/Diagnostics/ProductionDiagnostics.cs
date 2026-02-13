namespace ServePoint.Cadet.Diagnostics;

public static class ProductionDiagnostics
{
    public static string? LastError { get; set; }
    public static DateTime? LastErrorAtUtc { get; set; }
}