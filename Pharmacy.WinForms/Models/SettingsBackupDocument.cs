namespace Pharmacy.WinForms.Models;

/// <summary>JSON payload written by local settings backup.</summary>
internal sealed class SettingsBackupDocument
{
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public AppSettingsPersistence Settings { get; set; } = new();
}
