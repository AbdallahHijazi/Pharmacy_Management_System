using Pharmacy.WinForms.Models;

namespace Pharmacy.WinForms.Ui;

internal static class CustomerDisplayHelper
{
    public static string ResolveDisplayName(string? rawName)
    {
        if (string.IsNullOrWhiteSpace(rawName))
        {
            return "زبون بدون اسم";
        }

        var trimmed = rawName.Trim();
        return PosProductView.IsGeneratedTestName(trimmed) ? "زبون بدون اسم" : trimmed;
    }

    public static string ResolveInitials(string displayName)
    {
        if (string.Equals(displayName, "زبون بدون اسم", StringComparison.Ordinal))
        {
            return "؟";
        }

        var parts = displayName.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length >= 2)
        {
            return $"{char.ToUpperInvariant(parts[0][0])}{char.ToUpperInvariant(parts[1][0])}";
        }

        if (displayName.Length >= 2)
        {
            return displayName[..2].ToUpperInvariant();
        }

        return displayName.ToUpperInvariant();
    }

    public static string ResolvePhone(string? phone) =>
        string.IsNullOrWhiteSpace(phone) ? "لا يوجد رقم" : phone.Trim();

    public static string ResolveAddress(string? address) =>
        string.IsNullOrWhiteSpace(address) ? "—" : address.Trim();

    public static string ResolveDebtStatus(decimal debtAmount)
    {
        if (debtAmount <= 0)
        {
            return "لا يوجد ديون";
        }

        return $"دين: {PosFormatting.FormatMoneyCompact(debtAmount)}";
    }
}
