namespace Pharmacy.Application.Common.Interfaces;

public interface IUserPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string hashedPassword, string providedPassword);
}
