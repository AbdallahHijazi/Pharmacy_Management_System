using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Pharmacy.WinForms.Models;

namespace Pharmacy.WinForms.Services;

public readonly record struct ApiGetResult<T>(bool Success, T? Data, string? ErrorMessage, bool IsConnectionError, int? StatusCode)
    where T : class;

public sealed class ApiClient : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;

    public ApiClient()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(NormalizeBaseUrl(ApiConfiguration.BaseUrl)),
            Timeout = TimeSpan.FromSeconds(30)
        };
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public void SetBearerToken(string? token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = string.IsNullOrWhiteSpace(token)
            ? null
            : new AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>Re-applies the session token before authenticated API calls.</summary>
    public void EnsureSessionAuthorization()
    {
        SetBearerToken(SessionManager.Token);
    }

    public async Task<(bool Success, LoginResponse? Data, string? ErrorMessage, bool IsConnectionError)> PostLoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        SetBearerToken(null);
        var requestUri = BuildRequestUri("api/Auth/login");
        Debug.WriteLine($"[API/login] POST {requestUri} | BaseUrl={_httpClient.BaseAddress} | HasToken=false");

        try
        {
            using var response = await _httpClient.PostAsJsonAsync(
                "api/Auth/login",
                request,
                JsonOptions,
                cancellationToken);

            Debug.WriteLine($"[API/login] status={(int)response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<LoginResponse>(JsonOptions, cancellationToken);
                if (data is null || string.IsNullOrWhiteSpace(data.Token))
                {
                    return (false, null, "استجابة غير صالحة من الخادم.", false);
                }

                return (true, data, null, false);
            }

            var message = await TryReadErrorMessageAsync(response, cancellationToken);
            return (false, null, message, false);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return (false, null, "انتهت مهلة الاتصال بالخادم. تحقق من أن API يعمل.", true);
        }
        catch (HttpRequestException)
        {
            return (false, null, "تعذر الاتصال بالخادم. تأكد من تشغيل PharmacyProjectApi.", true);
        }
    }

    public Task<ApiGetResult<T>> GetAsync<T>(string relativeUrl, CancellationToken cancellationToken = default)
        where T : class
        => GetAsync<T>(relativeUrl, logContext: null, cancellationToken);

    public async Task<ApiGetResult<T>> GetAsync<T>(
        string relativeUrl,
        string? logContext,
        CancellationToken cancellationToken = default)
        where T : class
    {
        EnsureSessionAuthorization();

        var requestUri = BuildRequestUri(relativeUrl);
        var hasAuth = _httpClient.DefaultRequestHeaders.Authorization is not null;
        var hasToken = SessionManager.IsAuthenticated;
        Debug.WriteLine(
            $"[API/{logContext ?? "GET"}] {requestUri} | BaseUrl={_httpClient.BaseAddress} | HasToken={hasToken} | HasAuthHeader={hasAuth}");

        try
        {
            using var response = await _httpClient.GetAsync(relativeUrl, cancellationToken);
            var statusCode = (int)response.StatusCode;

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
                if (data is null)
                {
                    LogApi(logContext, requestUri, statusCode, "استجابة فارغة أو غير قابلة للتحليل.");
                    return new ApiGetResult<T>(false, null, "استجابة غير صالحة من الخادم.", false, statusCode);
                }

                LogApi(logContext, requestUri, statusCode, "OK");
                return new ApiGetResult<T>(true, data, null, false, statusCode);
            }

            var body = await SafeReadBodyAsync(response, cancellationToken);
            var message = await TryReadApiErrorMessageAsync(response, cancellationToken);
            LogApi(logContext, requestUri, statusCode, $"{message} | body: {Truncate(body, 300)}");
            return new ApiGetResult<T>(false, null, message, false, statusCode);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            LogApi(logContext, requestUri, null, "Timeout");
            return new ApiGetResult<T>(false, null, "انتهت مهلة الاتصال بالخادم. تحقق من أن API يعمل.", true, null);
        }
        catch (HttpRequestException ex)
        {
            LogApi(logContext, requestUri, null, $"HttpRequestException: {ex.Message}");
            return new ApiGetResult<T>(false, null, "تعذر الاتصال بالخادم. تأكد من تشغيل PharmacyProjectApi.", true, null);
        }
        catch (Exception ex)
        {
            LogApi(logContext, requestUri, null, ex.ToString());
            return new ApiGetResult<T>(false, null, "حدث خطأ غير متوقع أثناء الاتصال بالخادم.", false, null);
        }
    }

    private string BuildRequestUri(string relativeUrl)
    {
        var baseUri = _httpClient.BaseAddress?.ToString().TrimEnd('/') ?? ApiConfiguration.BaseUrl.TrimEnd('/');
        var path = relativeUrl.TrimStart('/');
        return $"{baseUri}/{path}";
    }

    private static async Task<string> SafeReadBodyAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch
        {
            return string.Empty;
        }
    }

    private static void LogApi(string? context, string url, int? statusCode, string detail)
    {
        var prefix = string.IsNullOrWhiteSpace(context) ? "API" : $"API/{context}";
        Debug.WriteLine($"[{prefix}] {url} | status={(statusCode?.ToString() ?? "—")} | {detail}");
    }

    private static string Truncate(string value, int max) =>
        value.Length <= max ? value : value[..max] + "…";

    private static async Task<string> TryReadErrorMessageAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var apiMessage = await TryReadApiErrorMessageAsync(response, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            return "البريد الإلكتروني أو كلمة المرور غير صحيحة.";
        }

        return apiMessage;
    }

    private static async Task<string> TryReadApiErrorMessageAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        try
        {
            var error = await response.Content.ReadFromJsonAsync<ApiErrorBody>(JsonOptions, cancellationToken);
            if (!string.IsNullOrWhiteSpace(error?.Message))
            {
                return error.Message;
            }
        }
        catch
        {
            // Ignore parse errors; fall back to generic message.
        }

        return response.StatusCode switch
        {
            System.Net.HttpStatusCode.Unauthorized => "غير مصرح (401).",
            System.Net.HttpStatusCode.Forbidden => "غير مسموح (403).",
            System.Net.HttpStatusCode.BadRequest => "طلب غير صالح (400).",
            System.Net.HttpStatusCode.NotFound => "غير موجود (404).",
            System.Net.HttpStatusCode.InternalServerError => "خطأ في الخادم (500).",
            _ => "تعذر إكمال الطلب."
        };
    }

    private static string NormalizeBaseUrl(string baseUrl)
    {
        var trimmed = baseUrl.Trim().TrimEnd('/');
        return trimmed.EndsWith('/') ? trimmed : trimmed + "/";
    }

    public void Dispose() => _httpClient.Dispose();
}
