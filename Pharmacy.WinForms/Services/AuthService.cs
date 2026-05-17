using Pharmacy.WinForms.Models;

namespace Pharmacy.WinForms.Services;

public sealed class AuthService
{
    private readonly ApiClient _apiClient;

    public AuthService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<AuthResult> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return AuthResult.Failed("يرجى إدخال البريد الإلكتروني.");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            return AuthResult.Failed("يرجى إدخال كلمة المرور.");
        }

        var request = new LoginRequest
        {
            Email = email.Trim(),
            Password = password
        };

        var (success, data, errorMessage, isConnectionError) =
            await _apiClient.PostLoginAsync(request, cancellationToken);

        if (!success || data is null)
        {
            return isConnectionError
                ? AuthResult.ConnectionFailed(errorMessage ?? "تعذر الاتصال بالخادم.")
                : AuthResult.Failed(errorMessage ?? "تعذر تسجيل الدخول.");
        }

        SessionManager.SetSession(data);
        _apiClient.SetBearerToken(data.Token);

        return AuthResult.Succeeded();
    }

    public void Logout()
    {
        SessionManager.Clear();
        _apiClient.SetBearerToken(null);
    }
}
