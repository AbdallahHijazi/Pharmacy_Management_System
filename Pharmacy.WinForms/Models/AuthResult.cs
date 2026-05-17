namespace Pharmacy.WinForms.Models;

public sealed class AuthResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public bool IsConnectionError { get; init; }

    public static AuthResult Succeeded() => new() { Success = true };

    public static AuthResult Failed(string message) => new()
    {
        Success = false,
        ErrorMessage = message
    };

    public static AuthResult ConnectionFailed(string message) => new()
    {
        Success = false,
        ErrorMessage = message,
        IsConnectionError = true
    };
}
