namespace Pharmacy.WinForms.Models;

internal sealed class ReportExportResult
{
    public bool Success { get; init; }
    public string? FilePath { get; init; }
    public string? ErrorMessage { get; init; }
    public bool IsCancelled { get; init; }

    public static ReportExportResult Cancelled() => new() { IsCancelled = true };

    public static ReportExportResult Ok(string filePath) =>
        new() { Success = true, FilePath = filePath };

    public static ReportExportResult Fail(string message) =>
        new() { Success = false, ErrorMessage = message };
}

internal sealed class ReportBulkExportResult
{
    public bool Success { get; init; }
    public string? FilePath { get; init; }
    public string? ErrorMessage { get; init; }
    public bool IsCancelled { get; init; }
    public int ExportedCount { get; init; }
    public int UnavailableCount { get; init; }

    public static ReportBulkExportResult Cancelled() => new() { IsCancelled = true };

    public static ReportBulkExportResult Ok(string filePath, int exported, int unavailable) =>
        new() { Success = true, FilePath = filePath, ExportedCount = exported, UnavailableCount = unavailable };

    public static ReportBulkExportResult Fail(string message) =>
        new() { Success = false, ErrorMessage = message };
}
