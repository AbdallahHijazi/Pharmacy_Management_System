using System.Text.Json;
using Pharmacy.WinForms.Models;

namespace Pharmacy.WinForms.Services;

internal static class LocalAppSettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static string DirectoryPath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PharmaCare");

    public static string FilePath => Path.Combine(DirectoryPath, "settings.json");

    public static SettingsFormState LoadOrDefault()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                return new SettingsFormState();
            }

            var json = File.ReadAllText(FilePath);
            var doc = JsonSerializer.Deserialize<AppSettingsPersistence>(json, JsonOptions);
            return doc?.ToState() ?? new SettingsFormState();
        }
        catch
        {
            return new SettingsFormState();
        }
    }

    /// <summary>
    /// Writes <c>PharmaCare_SettingsBackup_yyyyMMdd_HHmmss.json</c> under <paramref name="backupDirectory"/>.
    /// Uses current UI state; merges from existing settings.json when present.
    /// </summary>
    public static bool TryCreateBackup(
        string backupDirectory,
        SettingsFormState currentUiState,
        out string? createdFilePath,
        out string? error)
    {
        createdFilePath = null;
        error = null;

        if (string.IsNullOrWhiteSpace(backupDirectory))
        {
            error = "مسار النسخ الاحتياطي فارغ.";
            return false;
        }

        try
        {
            Directory.CreateDirectory(backupDirectory);

            var persistence = File.Exists(FilePath)
                ? JsonSerializer.Deserialize<AppSettingsPersistence>(File.ReadAllText(FilePath), JsonOptions)
                  ?? AppSettingsPersistence.FromState(currentUiState)
                : AppSettingsPersistence.FromState(currentUiState);

            var fromUi = AppSettingsPersistence.FromState(currentUiState);
            persistence.PharmacyName = fromUi.PharmacyName;
            persistence.Address = fromUi.Address;
            persistence.Phone = fromUi.Phone;
            persistence.CurrencyCode = fromUi.CurrencyCode;
            persistence.ExchangeRate = fromUi.ExchangeRate;
            persistence.ThemeIndex = fromUi.ThemeIndex;
            persistence.FontSizeLevel = fromUi.FontSizeLevel;
            persistence.ExpiryAlertDays = fromUi.ExpiryAlertDays;
            persistence.LowStockThreshold = fromUi.LowStockThreshold;
            persistence.BackupPath = fromUi.BackupPath;
            persistence.AutoBackupSchedule = fromUi.AutoBackupSchedule;

            var document = new SettingsBackupDocument
            {
                CreatedAtUtc = DateTime.UtcNow,
                Settings = persistence
            };

            var stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var fileName = $"PharmaCare_SettingsBackup_{stamp}.json";
            createdFilePath = Path.Combine(backupDirectory, fileName);

            var json = JsonSerializer.Serialize(document, JsonOptions);
            File.WriteAllText(createdFilePath, json);
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    /// <summary>Persists full UI settings. Returns true if file was written.</summary>
    public static bool TrySave(SettingsFormState state, out string? error)
    {
        error = null;
        try
        {
            Directory.CreateDirectory(DirectoryPath);
            var dto = AppSettingsPersistence.FromState(state);
            var json = JsonSerializer.Serialize(dto, JsonOptions);
            File.WriteAllText(FilePath, json);
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }
}
