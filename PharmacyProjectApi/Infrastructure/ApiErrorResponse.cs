namespace Pharmacy.API.Infrastructure
{
    public sealed class ApiErrorResponse
    {
        public string Message { get; init; } = string.Empty;
        public string ErrorCode { get; init; } = string.Empty;
        public IDictionary<string, string[]>? ValidationErrors { get; init; }
        public string TraceId { get; init; } = string.Empty;
    }
}


