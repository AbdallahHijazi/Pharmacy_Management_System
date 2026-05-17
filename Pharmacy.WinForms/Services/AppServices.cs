namespace Pharmacy.WinForms.Services;

internal static class AppServices
{
    public static ApiClient ApiClient { get; } = new();
    public static AuthService AuthService { get; } = new(ApiClient);
}
