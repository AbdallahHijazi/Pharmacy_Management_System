namespace Pharmacy.WinForms.Models;

/// <summary>DTO for JSON under %AppData%\PharmaCare\settings.json</summary>
internal sealed class AppSettingsPersistence
{
    public int Version { get; set; } = 1;
    public string PharmacyName { get; set; } = "صيدلية الشفاء";
    public string Address { get; set; } = "شارع الاستقلال, البناء 4";
    public string Phone { get; set; } = "011-234-5678";
    public string CurrencyCode { get; set; } = "SYP";
    public string ExchangeRate { get; set; } = "14500";
    public int ThemeIndex { get; set; }
    public int FontSizeLevel { get; set; } = 2;
    public string ExpiryAlertDays { get; set; } = "90";
    public string LowStockThreshold { get; set; } = "5";
    public string BackupPath { get; set; } = @"D:\PharmacyBackups";
    public string AutoBackupSchedule { get; set; } = "يومياً";

    public static AppSettingsPersistence FromState(SettingsFormState s) => new()
    {
        PharmacyName = s.PharmacyName,
        Address = s.Address,
        Phone = s.Phone,
        CurrencyCode = s.CurrencyCode,
        ExchangeRate = s.ExchangeRate,
        ThemeIndex = s.ThemeIndex,
        FontSizeLevel = s.FontSizeLevel,
        ExpiryAlertDays = s.ExpiryAlertDays,
        LowStockThreshold = s.LowStockThreshold,
        BackupPath = s.BackupPath,
        AutoBackupSchedule = s.AutoBackupSchedule
    };

    public SettingsFormState ToState() => new()
    {
        PharmacyName = PharmacyName,
        Address = Address,
        Phone = Phone,
        CurrencyCode = CurrencyCode,
        ExchangeRate = ExchangeRate,
        ThemeIndex = ThemeIndex,
        FontSizeLevel = FontSizeLevel,
        ExpiryAlertDays = ExpiryAlertDays,
        LowStockThreshold = LowStockThreshold,
        BackupPath = BackupPath,
        AutoBackupSchedule = AutoBackupSchedule
    };
}
