using Pharmacy.WinForms.Services;

namespace Pharmacy.WinForms.Ui;

internal static class PosFormatting
{
    public static string FormatMoney(decimal value)
    {
        var code = LocalAppSettingsStore.LoadOrDefault().CurrencyCode?.Trim().ToUpperInvariant();
        var suffix = code switch
        {
            "USD" => "USD",
            "JD" or "JOD" => "JD",
            _ => "ل.س"
        };
        return $"{value:N2} {suffix}";
    }

    public static string FormatMoneyCompact(decimal value)
    {
        var code = LocalAppSettingsStore.LoadOrDefault().CurrencyCode?.Trim().ToUpperInvariant();
        var suffix = code switch
        {
            "USD" => "USD",
            "JD" or "JOD" => "JD",
            _ => "ل.س"
        };
        return $"{value:N2} {suffix}";
    }
}
