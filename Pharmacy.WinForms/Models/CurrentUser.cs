namespace Pharmacy.WinForms.Models;

public sealed class CurrentUser
{
    public Guid UserId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public Guid BranchId { get; init; }

    public static CurrentUser FromLoginResponse(LoginResponse response) => new()
    {
        UserId = response.UserId,
        FullName = response.FullName,
        Email = response.Email,
        Role = response.Role,
        BranchId = response.BranchId
    };
}
