using Pharmacy.WinForms.Models;

namespace Pharmacy.WinForms.Services;

internal static class SettingsKeys
{
    public const string PharmacyName = "Pharmacy.Name";
    public const string PharmacyAddress = "Pharmacy.Address";
    public const string PharmacyPhone = "Pharmacy.Phone";
    public const string DefaultCurrency = "Currency.Default";
    public const string ExchangeRate = "Currency.ExchangeRate";
    public const string ExpiryAlertDays = "Alerts.ExpiryDays";
    public const string LowStockThreshold = "Alerts.LowStockThreshold";
}

internal sealed class SettingsService
{
    private readonly ApiClient _apiClient;

    public SettingsService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<SettingsLoadResult> LoadAsync(CancellationToken cancellationToken = default)
    {
        var defaults = new SettingsFormState();
        if (!SessionManager.IsAuthenticated)
        {
            return new SettingsLoadResult
            {
                State = defaults,
                UsedDefaults = true,
                ErrorMessage = "انتهت الجلسة. سجّل الدخول مرة أخرى."
            };
        }

        _apiClient.EnsureSessionAuthorization();
        var response = await _apiClient.GetAsync<List<SystemSettingApiModel>>(
            "api/v1/settings",
            "settings",
            cancellationToken).ConfigureAwait(false);

        if (!response.Success || response.Data is null)
        {
            return new SettingsLoadResult
            {
                State = defaults,
                UsedDefaults = true,
                ErrorMessage = response.IsConnectionError
                    ? "تعذر تحميل الإعدادات من الخادم. تم عرض القيم الافتراضية محلياً."
                    : response.ErrorMessage
            };
        }

        var byKey = response.Data
            .Where(s => !string.IsNullOrWhiteSpace(s.Key))
            .GroupBy(s => s.Key, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var state = defaults.Clone();
        ApplySetting(byKey, SettingsKeys.PharmacyName, v => state.PharmacyName = v);
        ApplySetting(byKey, SettingsKeys.PharmacyAddress, v => state.Address = v);
        ApplySetting(byKey, SettingsKeys.PharmacyPhone, v => state.Phone = v);
        ApplySetting(byKey, SettingsKeys.DefaultCurrency, v => state.CurrencyCode = v);
        ApplySetting(byKey, SettingsKeys.ExchangeRate, v => state.ExchangeRate = v);
        ApplySetting(byKey, SettingsKeys.ExpiryAlertDays, v => state.ExpiryAlertDays = v);
        ApplySetting(byKey, SettingsKeys.LowStockThreshold, v => state.LowStockThreshold = v);

        return new SettingsLoadResult
        {
            State = state,
            SettingsByKey = byKey,
            UsedDefaults = byKey.Count == 0
        };
    }

    public async Task<SettingsSaveResult> SaveAsync(
        SettingsFormState current,
        SettingsFormState baseline,
        IReadOnlyDictionary<string, SystemSettingApiModel> settingsByKey,
        CancellationToken cancellationToken = default)
    {
        if (!SessionManager.IsAuthenticated)
        {
            return new SettingsSaveResult { ErrorMessage = "انتهت الجلسة. سجّل الدخول مرة أخرى." };
        }

        var updates = BuildUpdates(current, baseline, settingsByKey);
        if (updates.Count == 0)
        {
            return new SettingsSaveResult
            {
                NoChanges = true,
                Message = "لا توجد تغييرات لحفظها على الخادم."
            };
        }

        _apiClient.EnsureSessionAuthorization();
        var saved = 0;
        string? lastError = null;

        foreach (var update in updates)
        {
            var result = await _apiClient.PutAsync(
                "api/v1/settings",
                new UpdateSystemSettingApiRequest
                {
                    SettingId = update.SettingId,
                    Value = update.Value
                },
                "settings/update",
                cancellationToken).ConfigureAwait(false);

            if (result.Success)
            {
                saved++;
            }
            else
            {
                lastError = result.ErrorMessage;
            }
        }

        if (saved == 0)
        {
            return new SettingsSaveResult
            {
                ErrorMessage = lastError ?? "تعذر حفظ الإعدادات على الخادم."
            };
        }

        return SettingsSaveResult.Success(saved, appearancePending: false);
    }

    private static List<(Guid SettingId, string Value)> BuildUpdates(
        SettingsFormState current,
        SettingsFormState baseline,
        IReadOnlyDictionary<string, SystemSettingApiModel> settingsByKey)
    {
        var list = new List<(Guid, string)>();

        TryAddUpdate(list, settingsByKey, SettingsKeys.PharmacyName, current.PharmacyName, baseline.PharmacyName);
        TryAddUpdate(list, settingsByKey, SettingsKeys.PharmacyAddress, current.Address, baseline.Address);
        TryAddUpdate(list, settingsByKey, SettingsKeys.PharmacyPhone, current.Phone, baseline.Phone);
        TryAddUpdate(list, settingsByKey, SettingsKeys.DefaultCurrency, current.CurrencyCode, baseline.CurrencyCode);
        TryAddUpdate(list, settingsByKey, SettingsKeys.ExchangeRate, current.ExchangeRate, baseline.ExchangeRate);
        TryAddUpdate(list, settingsByKey, SettingsKeys.ExpiryAlertDays, current.ExpiryAlertDays, baseline.ExpiryAlertDays);
        TryAddUpdate(list, settingsByKey, SettingsKeys.LowStockThreshold, current.LowStockThreshold, baseline.LowStockThreshold);

        return list;
    }

    private static void TryAddUpdate(
        List<(Guid SettingId, string Value)> list,
        IReadOnlyDictionary<string, SystemSettingApiModel> settingsByKey,
        string key,
        string currentValue,
        string baselineValue)
    {
        if (string.Equals(currentValue.Trim(), baselineValue.Trim(), StringComparison.Ordinal))
        {
            return;
        }

        if (!settingsByKey.TryGetValue(key, out var setting))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(currentValue))
        {
            return;
        }

        list.Add((setting.SettingId, currentValue.Trim()));
    }

    private static void ApplySetting(
        IReadOnlyDictionary<string, SystemSettingApiModel> byKey,
        string key,
        Action<string> apply)
    {
        if (byKey.TryGetValue(key, out var setting) && !string.IsNullOrWhiteSpace(setting.Value))
        {
            apply(setting.Value);
        }
    }
}
