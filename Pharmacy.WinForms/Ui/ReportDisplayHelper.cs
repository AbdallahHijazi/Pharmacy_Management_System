using Pharmacy.WinForms.Models;

namespace Pharmacy.WinForms.Ui;

internal static class ReportDisplayHelper
{
    public static string FormatMoney(decimal value) => PosFormatting.FormatMoneyCompact(value);

    public static string FormatMoneyDisplay(decimal value) =>
        PosFormatting.FormatMoneyCompact(value);

    public static string FormatPercent(decimal? value) =>
        value.HasValue ? $"\u200E{value.Value:N1}%" : "—";

    public static string FormatQuantity(int value) => $"\u200E{value:N0}";

    public static string FormatDate(DateTime date) => date.ToString("yyyy-MM-dd");

    public static string FormatMonthPeriod(int year, int month) =>
        $"{year}-{month:D2}";

    public static string GetSingleExportDefaultFileName(ReportKind kind)
    {
        var stamp = DateTime.Now.ToString("yyyy-MM-dd");
        return kind switch
        {
            ReportKind.Sales => $"sales-report-{stamp}.csv",
            ReportKind.TopSellingProducts => $"top-selling-products-{stamp}.csv",
            ReportKind.ProfitLoss => $"financial-monthly-report-{DateTime.Now:yyyy-MM}.csv",
            ReportKind.ExpiringMedicines => $"inventory-expiry-report-{stamp}.csv",
            ReportKind.CustomerDebts => $"customer-debts-{stamp}.csv",
            ReportKind.SupplierPayables => $"supplier-payables-{stamp}.csv",
            _ => $"report-{stamp}.csv"
        };
    }

    public static string GetBulkExportDefaultFileName() =>
        $"pharmacy-reports-export-{DateTime.Now:yyyyMMdd-HHmm}.zip";
}
