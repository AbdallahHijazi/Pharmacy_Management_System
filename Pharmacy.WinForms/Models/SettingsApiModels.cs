namespace Pharmacy.WinForms.Models;

internal sealed class SystemSettingApiModel
{
    public Guid SettingId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

internal sealed class UpdateSystemSettingApiRequest
{
    public Guid SettingId { get; set; }
    public string Value { get; set; } = string.Empty;
}

internal sealed class SettingsLoadResult
{
    public required SettingsFormState State { get; init; }
    public IReadOnlyDictionary<string, SystemSettingApiModel> SettingsByKey { get; init; }
        = new Dictionary<string, SystemSettingApiModel>();
    public string? ErrorMessage { get; init; }
    public bool UsedDefaults { get; init; }
}

internal sealed class SettingsSaveResult
{
    public bool AnySaved { get; init; }
    public bool NotSupported { get; init; }
    public bool NoChanges { get; init; }
    public string? Message { get; init; }
    public string? ErrorMessage { get; init; }
    public bool HasUnsupportedAppearanceChanges { get; init; }

    public static SettingsSaveResult Unsupported(string? detail = null) => new()
    {
        NotSupported = true,
        Message = detail ?? "حفظ إعدادات النظام غير مدعوم بعد في الواجهة الحالية."
    };

    public static SettingsSaveResult Success(int count, bool appearancePending) => new()
    {
        AnySaved = true,
        Message = appearancePending
            ? $"تم حفظ {count} إعداد(ات) على الخادم.\nتغييرات المظهر وحجم الخط لم تُحفظ بعد (غير مدعومة حالياً)."
            : $"تم حفظ {count} إعداد(ات) بنجاح.",
        HasUnsupportedAppearanceChanges = appearancePending
    };
}
