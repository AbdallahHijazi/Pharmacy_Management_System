using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Models;

internal sealed class UserListItemView
{
    public Guid Id { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string Initials { get; init; } = "؟";
    public string RoleName { get; init; } = "غير محدد";
    public Guid RoleId { get; init; }
    public bool IsActive { get; init; }
    public string StatusText { get; init; } = "غير معروف";
    public string LastLoginText { get; init; } = "غير متوفر";
    public DateTime? LastLoginAt { get; init; }
    public string ShortId { get; init; } = string.Empty;

    public static UserListItemView FromApi(UserListItemApiModel api) => new()
    {
        Id = api.UserId,
        DisplayName = UserDisplayHelper.ResolveDisplayName(api.FullName, api.Email),
        Email = api.Email?.Trim() ?? string.Empty,
        Phone = api.Phone?.Trim() ?? string.Empty,
        Initials = UserDisplayHelper.GetInitials(api.FullName, api.Email),
        RoleName = UserDisplayHelper.ResolveRoleName(api.Role),
        IsActive = api.IsActive,
        StatusText = UserDisplayHelper.FormatStatus(api.IsActive),
        LastLoginText = UserDisplayHelper.FormatLastLogin(api.LastLoginAt),
        LastLoginAt = api.LastLoginAt,
        ShortId = UserDisplayHelper.FormatShortId(api.UserId)
    };

    public static UserListItemView FromDetails(UserDetailsApiModel api) => new()
    {
        Id = api.UserId,
        DisplayName = UserDisplayHelper.ResolveDisplayName(api.FullName, api.Email),
        Email = api.Email?.Trim() ?? string.Empty,
        Phone = api.Phone?.Trim() ?? string.Empty,
        Initials = UserDisplayHelper.GetInitials(api.FullName, api.Email),
        RoleName = UserDisplayHelper.ResolveRoleName(api.Role),
        RoleId = api.RoleId,
        IsActive = api.IsActive,
        StatusText = UserDisplayHelper.FormatStatus(api.IsActive),
        LastLoginText = UserDisplayHelper.FormatLastLogin(api.LastLoginAt),
        LastLoginAt = api.LastLoginAt,
        ShortId = UserDisplayHelper.FormatShortId(api.UserId)
    };
}

internal sealed class RoleListItemView
{
    public Guid RoleId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;

    public static RoleListItemView FromApi(RoleListItemApiModel api) => new()
    {
        RoleId = api.RoleId,
        Name = api.Name?.Trim() ?? string.Empty,
        Description = api.Description?.Trim() ?? string.Empty
    };
}

internal sealed class UserStatsView
{
    public int TotalUsers { get; init; }
    public int ActivePharmacists { get; init; }
    public int SystemAdmins { get; init; }
    public bool HasPharmacistRole { get; init; }
}

internal sealed class UsersLoadState
{
    public bool Success { get; init; }
    public IReadOnlyList<UserListItemView> Users { get; init; } = Array.Empty<UserListItemView>();
    public string? ErrorMessage { get; init; }
    public bool IsConnectionError { get; init; }
}

internal sealed class MutateUserResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public bool IsConnectionError { get; init; }
    public UserListItemView? User { get; init; }
}

internal sealed class RolesLoadResult
{
    public bool Success { get; init; }
    public IReadOnlyList<RoleListItemView> Roles { get; init; } = Array.Empty<RoleListItemView>();
    public string? ErrorMessage { get; init; }
}
