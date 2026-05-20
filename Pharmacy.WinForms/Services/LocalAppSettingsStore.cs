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
