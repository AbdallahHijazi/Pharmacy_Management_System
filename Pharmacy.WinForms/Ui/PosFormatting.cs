using Pharmacy.WinForms.Services;

namespace Pharmacy.WinForms.Ui;

internal static class PosFormatting
{
    public static string CurrencySuffix
    {
        get
        {
            var code = LocalAppSettingsStore.LoadOrDefault().CurrencyCode?.Trim().ToUpperInvariant();
            return code switch
            {
                "USD" => "USD",
                "JD" or "JOD" => "JD",
                _ => "ل.س"
            };
        }
    }

    public static string FormatMoney(decimal value) => $"{value:N2} {CurrencySuffix}";

    public static string FormatMoneyCompact(decimal value) => FormatMoney(value);
}
