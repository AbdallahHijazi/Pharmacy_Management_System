namespace Pharmacy.WinForms.Models;

internal sealed class ApiErrorBody
{
    public string Message { get; init; } = string.Empty;
    public string ErrorCode { get; init; } = string.Empty;
}
