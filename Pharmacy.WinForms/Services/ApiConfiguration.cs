namespace Pharmacy.WinForms.Services;

/// <summary>
/// Change <see cref="BaseUrl"/> to point at your running API instance.
/// Default matches PharmacyProjectApi http profile (launchSettings.json).
/// </summary>
public static class ApiConfiguration
{
    public static string BaseUrl { get; set; } = "http://localhost:5075";
}
