namespace Pharmacy.Application.Common.Interfaces
{
    public interface IAuthSettingsProvider
    {
        string Secret { get; }
        string Issuer { get; }
        string Audience { get; }
        int ExpiryMinutes { get; }
    }
}