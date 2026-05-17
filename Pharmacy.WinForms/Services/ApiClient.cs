using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Pharmacy.WinForms.Models;

namespace Pharmacy.WinForms.Services;

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

    public async Task<(bool Success, LoginResponse? Data, string? ErrorMessage, bool IsConnectionError)> PostLoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.PostAsJsonAsync(
                "api/Auth/login",
                request,
                JsonOptions,
                cancellationToken);

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

    public async Task<(bool Success, T? Data, string? ErrorMessage, bool IsConnectionError)> GetAsync<T>(
        string relativeUrl,
        CancellationToken cancellationToken = default)
        where T : class
    {
        try
        {
            using var response = await _httpClient.GetAsync(relativeUrl, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
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

    private static async Task<string> TryReadErrorMessageAsync(
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
            System.Net.HttpStatusCode.Unauthorized =>
                "البريد الإلكتروني أو كلمة المرور غير صحيحة.",
            System.Net.HttpStatusCode.BadRequest =>
                "بيانات تسجيل الدخول غير صالحة.",
            _ => "تعذر تسجيل الدخول. حاول مرة أخرى."
        };
    }

    private static string NormalizeBaseUrl(string baseUrl)
    {
        var trimmed = baseUrl.Trim().TrimEnd('/');
        return trimmed.EndsWith('/') ? trimmed : trimmed + "/";
    }

    public void Dispose() => _httpClient.Dispose();
}
