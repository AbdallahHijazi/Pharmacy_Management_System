namespace Pharmacy.WinForms.Services;

/// <summary>
/// Change <see cref="BaseUrl"/> to point at your running API instance.
/// </summary>
/// <remarks>
/// Prefer HTTPS when the API runs with the Visual Studio "https" profile.
/// Requests to http://localhost:5075 may be redirected to HTTPS; HttpClient does not
/// forward the Authorization header on redirect, which causes 401 on dashboard calls.
/// HTTP-only profile: use http://localhost:5075
/// HTTPS profile: use https://localhost:7239
/// </remarks>
public static class ApiConfiguration
{
    public static string BaseUrl { get; set; } = "https://localhost:7239";
}
