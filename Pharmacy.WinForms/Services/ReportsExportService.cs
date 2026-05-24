using System.IO.Compression;
using System.Text;
using Pharmacy.WinForms.Models;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Services;

internal sealed class ReportsExportService
{
    private readonly ReportsService _reportsService;

    public ReportsExportService(ReportsService reportsService)
    {
        _reportsService = reportsService;
    }

    public async Task<ReportExportResult> ExportSingleReportAsync(
        ReportKind kind,
        string filePath,
        ReportLoadResult? cachedResult = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var card = ReportCatalog.CreateCards().First(c => c.Kind == kind);
            var load = cachedResult is { Success: true }
                ? cachedResult
                : await _reportsService.LoadReportAsync(kind, cancellationToken).ConfigureAwait(false);

            if (!load.IsAvailable)
            {
                return ReportExportResult.Fail(load.ErrorMessage ?? "هذا التقرير غير متاح من API.");
            }

            if (!load.Success)
            {
                return ReportExportResult.Fail(load.ErrorMessage ?? "تعذر تحميل بيانات التقرير من API.");
            }

            var csv = BuildCsv(card.Title, load);
            WriteUtf8BomCsv(filePath, csv);
            return ReportExportResult.Ok(filePath);
        }
        catch (OperationCanceledException)
        {
            return ReportExportResult.Fail("تم إلغاء عملية التصدير.");
        }
        catch (IOException)
        {
            return ReportExportResult.Fail("الملف مستخدم من برنامج آخر. أغلق الملف ثم أعد المحاولة.");
        }
        catch (UnauthorizedAccessException)
        {
            return ReportExportResult.Fail("لا توجد صلاحية للكتابة في المسار المحدد.");
        }
        catch (Exception ex)
        {
            return ReportExportResult.Fail($"تعذر تصدير التقرير: {ex.Message}");
        }
    }

    public async Task<ReportBulkExportResult> ExportAllReportsAsync(
        string zipFilePath,
        CancellationToken cancellationToken = default)
    {
        var unavailable = new List<string>();
        var exported = 0;
        var exportedAt = DateTime.Now;

        try
        {
            if (File.Exists(zipFilePath))
            {
                File.Delete(zipFilePath);
            }

            using (var archive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
            {
                foreach (var card in ReportCatalog.CreateCards())
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        var load = await _reportsService.LoadReportAsync(card.Kind, cancellationToken)
                            .ConfigureAwait(false);

                        if (!load.IsAvailable || !load.Success)
                        {
                            unavailable.Add($"{card.Title}: {load.ErrorMessage ?? "غير متوفر"}");
                            continue;
                        }

                        var csv = BuildCsv(card.Title, load);
                        var entry = archive.CreateEntry(GetZipEntryFileName(card.Kind), CompressionLevel.Optimal);
                        await using var entryStream = entry.Open();
                        await using var writer = new StreamWriter(entryStream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
                        await writer.WriteAsync(csv).ConfigureAwait(false);
                        exported++;
                    }
                    catch (Exception ex)
                    {
                        unavailable.Add($"{card.Title}: {ex.Message}");
                    }
                }

                var summary = BuildExportSummary(exportedAt, exported, unavailable);
                await WriteZipTextEntryAsync(archive, "export-summary.txt", summary, cancellationToken)
                    .ConfigureAwait(false);

                if (unavailable.Count > 0)
                {
                    var unavailableText = BuildUnavailableReportText(unavailable);
                    await WriteZipTextEntryAsync(archive, "unavailable-reports.txt", unavailableText, cancellationToken)
                        .ConfigureAwait(false);
                }
            }

            if (exported == 0)
            {
                return ReportBulkExportResult.Fail("تعذر تصدير أي تقرير. راجع unavailable-reports.txt داخل الأرشيف.");
            }

            return ReportBulkExportResult.Ok(zipFilePath, exported, unavailable.Count);
        }
        catch (OperationCanceledException)
        {
            return ReportBulkExportResult.Fail("تم إلغاء عملية التصدير.");
        }
        catch (IOException)
        {
            return ReportBulkExportResult.Fail("الملف مستخدم من برنامج آخر. أغلق الملف ثم أعد المحاولة.");
        }
        catch (UnauthorizedAccessException)
        {
            return ReportBulkExportResult.Fail("لا توجد صلاحية للكتابة في المسار المحدد.");
        }
        catch (Exception ex)
        {
            return ReportBulkExportResult.Fail($"تعذر إنشاء التصدير الشامل: {ex.Message}");
        }
    }

    internal static string BuildCsv(string reportTitle, ReportLoadResult load)
    {
        var sb = new StringBuilder();
        var exportedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        var pharmacyName = UiBranding.PharmacyDisplayName;

        AppendRow(sb, "اسم التقرير", reportTitle);
        AppendRow(sb, "تاريخ التصدير", exportedAt);
        if (!string.IsNullOrWhiteSpace(load.PeriodText))
        {
            AppendRow(sb, "الفترة", load.PeriodText);
        }

        AppendRow(sb, "اسم الصيدلية", pharmacyName);
        sb.AppendLine();

        var content = load.Content;
        if (content is null)
        {
            AppendRow(sb, "ملاحظة", "لا توجد بيانات للتصدير.");
            return sb.ToString();
        }

        if (content.Summary.Count > 0)
        {
            AppendRow(sb, "البند", "القيمة");
            foreach (var (label, value) in content.Summary)
            {
                AppendRow(sb, label, SanitizeExportValue(value));
            }

            sb.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(content.EmptyMessage))
        {
            AppendRow(sb, "ملاحظة", content.EmptyMessage);
            sb.AppendLine();
        }

        if (content.TableHeaders.Count > 0)
        {
            AppendRow(sb, content.TableHeaders.ToArray());
            foreach (var row in content.TableRows)
            {
                var cells = content.TableHeaders
                    .Select((_, index) => index < row.Cells.Count ? SanitizeExportValue(row.Cells[index]) : string.Empty)
                    .ToArray();
                AppendRow(sb, cells);
            }
        }

        return sb.ToString();
    }

    internal static void WriteUtf8BomCsv(string path, string csv)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(path, csv, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
    }

    private static string BuildExportSummary(DateTime exportedAt, int exportedCount, IReadOnlyList<string> unavailable)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Pharmacy Reports Export");
        sb.AppendLine($"Exported At: {exportedAt:yyyy-MM-dd HH:mm}");
        sb.AppendLine($"Pharmacy: {UiBranding.PharmacyDisplayName}");
        sb.AppendLine($"Reports Exported: {exportedCount}");
        sb.AppendLine($"Reports Unavailable: {unavailable.Count}");
        if (unavailable.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("See unavailable-reports.txt for details.");
        }

        return sb.ToString();
    }

    private static string BuildUnavailableReportText(IReadOnlyList<string> unavailable)
    {
        var sb = new StringBuilder();
        sb.AppendLine("التقارير غير المتاحة أثناء التصدير الشامل");
        sb.AppendLine(new string('-', 48));
        foreach (var line in unavailable)
        {
            sb.AppendLine(line);
        }

        return sb.ToString();
    }

    private static async Task WriteZipTextEntryAsync(
        ZipArchive archive,
        string entryName,
        string content,
        CancellationToken cancellationToken)
    {
        var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
        await using var stream = entry.Open();
        await using var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
        cancellationToken.ThrowIfCancellationRequested();
        await writer.WriteAsync(content).ConfigureAwait(false);
    }

    private static void AppendRow(StringBuilder sb, params string[] cells)
    {
        sb.AppendLine(string.Join(",", cells.Select(EscapeCsv)));
    }

    private static void AppendRow(StringBuilder sb, string label, string value) =>
        AppendRow(sb, new[] { label, value });

    private static string EscapeCsv(string? value)
    {
        var text = SanitizeExportValue(value);
        if (text.Contains('"'))
        {
            text = text.Replace("\"", "\"\"");
        }

        if (text.Contains(',') || text.Contains('\n') || text.Contains('\r') || text.Contains('"'))
        {
            return $"\"{text}\"";
        }

        return $"\"{text}\"";
    }

    private static string SanitizeExportValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "غير متوفر";
        }

        return value.Trim().TrimStart('\u200E');
    }

    private static string GetZipEntryFileName(ReportKind kind) => kind switch
    {
        ReportKind.Sales => "sales-report.csv",
        ReportKind.TopSellingProducts => "top-selling-products.csv",
        ReportKind.ProfitLoss => "financial-monthly-report.csv",
        ReportKind.ExpiringMedicines => "inventory-expiry-report.csv",
        ReportKind.CustomerDebts => "customer-debts.csv",
        ReportKind.SupplierPayables => "supplier-payables.csv",
        _ => "report.csv"
    };
}
