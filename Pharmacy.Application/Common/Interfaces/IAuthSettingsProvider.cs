namespace Pharmacy.Application.Common.Interfaces;

public interface IAuthSettingsProvider
{
    int RefreshTokenExpiryDays { get; }
}
