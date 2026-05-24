namespace Pharmacy.WinForms.Ui;

internal static class SupplierDisplayHelper
{
    public static string ResolveSupplierDisplayName(string? rawName) =>
        PurchaseDisplayHelper.ResolveSupplierDisplayName(rawName);

    public static string ResolveSupplierSubtitle(string? rawName, string displayName) =>
        PurchaseDisplayHelper.ResolveSupplierSubtitle(rawName, displayName);

    public static string ResolveInitials(string displayName)
    {
        if (string.Equals(displayName, "مورد بدون اسم", StringComparison.Ordinal))
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

    public static string ResolveContactPerson(string? contactPerson) =>
        string.IsNullOrWhiteSpace(contactPerson) ? "غير متوفر" : contactPerson.Trim();

    public static string ResolvePhone(string? phone) =>
        string.IsNullOrWhiteSpace(phone) ? "لا يوجد رقم" : phone.Trim();

    public static string ResolveAddress(string? address) =>
        string.IsNullOrWhiteSpace(address) ? "—" : address.Trim();

    public static string FormatMoney(decimal? amount) =>
        amount.HasValue ? PosFormatting.FormatMoneyCompact(amount.Value) : "غير متوفر";

    public static string FormatPayable(decimal payableAmount) =>
        payableAmount > 0
            ? PosFormatting.FormatMoneyCompact(payableAmount)
            : "لا توجد مستحقات";

    public static string FormatCount(int count) => $"\u200E{count:N0}";

    public static string FormatStatDisplay(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || string.Equals(value, "غير متوفر", StringComparison.Ordinal))
        {
            return "غير متوفر";
        }

        return value.StartsWith("\u200E", StringComparison.Ordinal) ? value : "\u200E" + value;
    }

    public static bool IsNumericDisplay(string value) =>
        !string.IsNullOrWhiteSpace(value)
        && value != "غير متوفر"
        && value != "لا توجد مستحقات";
}
