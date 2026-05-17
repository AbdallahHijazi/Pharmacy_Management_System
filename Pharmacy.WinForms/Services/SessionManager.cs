using Pharmacy.WinForms.Models;

namespace Pharmacy.WinForms.Services;

public static class SessionManager
{
    public static string? Token { get; private set; }
    public static CurrentUser? CurrentUser { get; private set; }

    public static bool IsAuthenticated => !string.IsNullOrWhiteSpace(Token);

    public static void SetSession(LoginResponse response)
    {
        Token = response.Token;
        CurrentUser = CurrentUser.FromLoginResponse(response);
    }

    public static void Clear()
    {
        Token = null;
        CurrentUser = null;
    }
}
