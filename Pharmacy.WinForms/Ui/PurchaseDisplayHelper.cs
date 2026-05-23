using Pharmacy.WinForms.Models;

namespace Pharmacy.WinForms.Ui;

internal static class PurchaseDisplayHelper
{
    public static string ResolveSupplierDisplayName(string? rawName)
    {
        if (string.IsNullOrWhiteSpace(rawName))
        {
            return "مورد بدون اسم";
        }

        var trimmed = rawName.Trim();
        return PosProductView.IsGeneratedTestName(trimmed) ? "مورد بدون اسم" : trimmed;
    }

    public static string ResolveSupplierSubtitle(string? rawName, string displayName)
    {
        if (!string.Equals(displayName, "مورد بدون اسم", StringComparison.Ordinal)
            || string.IsNullOrWhiteSpace(rawName))
        {
            return string.Empty;
        }

        return $"الكود: {rawName.Trim()}";
    }
}
