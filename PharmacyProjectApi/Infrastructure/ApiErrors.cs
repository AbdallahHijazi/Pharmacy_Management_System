using Microsoft.AspNetCore.Mvc;
using Pharmacy.Domain.Exceptions;

namespace Pharmacy.API.Infrastructure
{
    internal static class ApiErrors
    {
        public static (int StatusCode, ApiErrorResponse Body) Map(Exception ex, string traceId) => ex switch
        {
            ValidationException v => (StatusCodes.Status400BadRequest, Build(v.Message, "VALIDATION_ERROR", traceId, v.Errors)),
            NotFoundException n => (StatusCodes.Status404NotFound, Build(n.Message, "NOT_FOUND", traceId)),
            BadRequestException b => (StatusCodes.Status400BadRequest, Build(b.Message, "BAD_REQUEST", traceId)),
            UnauthorizedException u => (StatusCodes.Status401Unauthorized, Build(u.Message, "UNAUTHORIZED", traceId)),
            StatusAlreadyExistsException s => (StatusCodes.Status409Conflict, Build(s.Message, "CONFLICT", traceId)),
            InvalidOperationException i => (StatusCodes.Status400BadRequest, Build(i.Message, "INVALID_OPERATION", traceId)),
            _ => (StatusCodes.Status500InternalServerError, Build("حدث خطأ غير متوقع.", "INTERNAL_SERVER_ERROR", traceId))
        };

        public static IActionResult From(Exception ex)
        {
            var (statusCode, body) = Map(ex, string.Empty);
            return new ObjectResult(body) { StatusCode = statusCode };
        }

        private static ApiErrorResponse Build(
            string message,
            string errorCode,
            string traceId,
            IReadOnlyDictionary<string, string[]>? validationErrors = null)
        {
            return new ApiErrorResponse
            {
                Message = message,
                ErrorCode = errorCode,
                ValidationErrors = validationErrors is null ? null : new Dictionary<string, string[]>(validationErrors),
                TraceId = traceId
            };
        }
    }
}


