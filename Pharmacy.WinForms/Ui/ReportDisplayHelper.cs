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
}
