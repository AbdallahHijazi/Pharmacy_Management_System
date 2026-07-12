using Pharmacy.WinForms.Models;

namespace Pharmacy.WinForms.Ui;

internal static class UserDisplayHelper
{
    public static string ResolveDisplayName(string? fullName, string? email)
    {
        if (!string.IsNullOrWhiteSpace(fullName))
        {
            return fullName.Trim();
        }

        if (!string.IsNullOrWhiteSpace(email))
        {
            return email.Trim();
        }

        return "مستخدم بدون اسم";
    }

    public static string ResolveRoleName(string? role) =>
        string.IsNullOrWhiteSpace(role) ? "غير محدد" : role.Trim();

    public static string GetInitials(string? fullName, string? email)
    {
        var source = !string.IsNullOrWhiteSpace(fullName) ? fullName.Trim() : email?.Trim();
        if (string.IsNullOrWhiteSpace(source))
        {
            return "؟";
        }

        var parts = source.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length >= 2)
        {
            return $"{char.ToUpperInvariant(parts[0][0])}{char.ToUpperInvariant(parts[^1][0])}";
        }

        return source.Length >= 2
            ? source[..2].ToUpperInvariant()
            : source[..1].ToUpperInvariant();
    }

    public static string FormatShortId(Guid userId) =>
        userId.ToString("N")[..8].ToUpperInvariant();

    public static string FormatLastLogin(DateTime? lastLoginAt) =>
        lastLoginAt.HasValue ? lastLoginAt.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm") : "غير متوفر";

    public static string FormatStatus(bool? isActive) => isActive switch
    {
        true => "نشط",
        false => "غير نشط",
        _ => "غير معروف"
    };

    public static (Color Back, Color Fore) GetRoleBadgeColors(string roleName)
    {
        var normalized = roleName.Trim().ToLowerInvariant();
        if (normalized.Contains("admin"))
        {
            return (PharmaTheme.WithAlpha(PharmaTheme.PrimaryContainer, 180), PharmaTheme.PrimaryDark);
        }

        if (normalized.Contains("pharmacist") || normalized.Contains("صيد"))
        {
            return (PharmaTheme.WithAlpha(PharmaTheme.SuccessSurface, 200), PharmaTheme.Success);
        }

        if (normalized.Contains("cashier") || normalized.Contains("صند"))
        {
            return (PharmaTheme.WithAlpha(PharmaTheme.WarningSurface, 200), PharmaTheme.Warning);
        }

        return (PharmaTheme.SurfaceContainerHigh, PharmaTheme.OnSurfaceVariant);
    }

    public static bool IsAdminRole(string roleName) =>
        roleName.Contains("admin", StringComparison.OrdinalIgnoreCase);

    public static bool IsPharmacistRole(string roleName) =>
        roleName.Contains("pharmacist", StringComparison.OrdinalIgnoreCase)
        || roleName.Contains("صيد", StringComparison.OrdinalIgnoreCase);

    public static UserStatsView ComputeStats(IReadOnlyList<UserListItemView> users)
    {
        var hasPharmacistRole = users.Any(u => IsPharmacistRole(u.RoleName));
        return new UserStatsView
        {
            TotalUsers = users.Count,
            ActivePharmacists = users.Count(u => u.IsActive && IsPharmacistRole(u.RoleName)),
            SystemAdmins = users.Count(u => u.IsActive && IsAdminRole(u.RoleName)),
            HasPharmacistRole = hasPharmacistRole
        };
    }

    public static bool MatchesSearch(UserListItemView user, string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return true;
        }

        var q = query.Trim();
        return user.DisplayName.Contains(q, StringComparison.OrdinalIgnoreCase)
               || user.Email.Contains(q, StringComparison.OrdinalIgnoreCase)
               || user.RoleName.Contains(q, StringComparison.OrdinalIgnoreCase)
               || user.ShortId.Contains(q, StringComparison.OrdinalIgnoreCase)
               || user.Phone.Contains(q, StringComparison.OrdinalIgnoreCase);
    }
}
