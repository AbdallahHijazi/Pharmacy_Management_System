namespace Pharmacy.WinForms.Models;

/// <summary>Pharmacy display name shown in shell (sidebar); persisted via local settings / API.</summary>
internal static class UiBranding
{
    public static event EventHandler? PharmacyDisplayNameChanged;

    public static string PharmacyDisplayName { get; private set; } = "صيدلية الشفاء";

    public static void InitializeFromLocal(SettingsFormState local)
    {
        if (!string.IsNullOrWhiteSpace(local.PharmacyName))
        {
            PharmacyDisplayName = local.PharmacyName.Trim();
        }
    }

    public static void SetPharmacyDisplayName(string? name)
    {
        var next = string.IsNullOrWhiteSpace(name) ? PharmacyDisplayName : name.Trim();
        if (string.Equals(PharmacyDisplayName, next, StringComparison.Ordinal))
        {
            return;
        }

        PharmacyDisplayName = next;
        PharmacyDisplayNameChanged?.Invoke(null, EventArgs.Empty);
    }
}
