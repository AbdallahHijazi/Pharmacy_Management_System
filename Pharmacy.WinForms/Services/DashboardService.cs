using System.Diagnostics;
using Pharmacy.WinForms.Models;

namespace Pharmacy.WinForms.Services;

public sealed class DashboardService
{
    private readonly ApiClient _apiClient;

    public DashboardService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<DashboardLoadResult> LoadDashboardAsync(CancellationToken cancellationToken = default)
    {
        var baseUrl = ApiConfiguration.BaseUrl;
        var hasToken = SessionManager.IsAuthenticated;
        Debug.WriteLine($"[Dashboard] Load start | BaseUrl={baseUrl} | HasToken={hasToken}");

        if (!hasToken)
        {
            return new DashboardLoadResult
            {
                Summary = CreateEmptySummary(),
                IsMockData = false,
                ErrorMessage = "انتهت الجلسة. سجّل الدخول مرة أخرى."
            };
        }

        _apiClient.EnsureSessionAuthorization();

        var today = DateTime.UtcNow.Date;
        var dateQuery = today.ToString("yyyy-MM-dd");

        var stats = await _apiClient.GetAsync<DashboardStatsApiModel>(
            "api/v1/dashboard/stats", "dashboard/stats", cancellationToken).ConfigureAwait(false);
        var sales = await _apiClient.GetAsync<List<LatestSalesInvoiceApiModel>>(
            "api/v1/dashboard/latest-sales-invoices", "dashboard/latest-sales", cancellationToken).ConfigureAwait(false);
        var lowStock = await _apiClient.GetAsync<List<LowStockProductApiModel>>(
            "api/v1/dashboard/low-stock-products", "dashboard/low-stock", cancellationToken).ConfigureAwait(false);
        var expiring = await _apiClient.GetAsync<List<ExpiringSoonBatchApiModel>>(
            "api/v1/dashboard/expiring-soon-batches", "dashboard/expiring", cancellationToken).ConfigureAwait(false);
        var profit = await _apiClient.GetAsync<DailyFinancialReportApiModel>(
            $"api/v1/reports/financial/daily?date={dateQuery}", "reports/financial/daily", cancellationToken).ConfigureAwait(false);

        if (stats.IsConnectionError || sales.IsConnectionError || lowStock.IsConnectionError || expiring.IsConnectionError)
        {
            return new DashboardLoadResult
            {
                Summary = CreateEmptySummary(),
                IsMockData = false,
                ErrorMessage = BuildConnectionErrorMessage(stats, sales, lowStock, expiring)
            };
        }

        if (IsUnauthorized(stats.StatusCode, sales.StatusCode, lowStock.StatusCode, expiring.StatusCode, profit.StatusCode))
        {
            Debug.WriteLine(
                $"[Dashboard] Unauthorized | stats={stats.StatusCode} sales={sales.StatusCode} lowStock={lowStock.StatusCode} expiring={expiring.StatusCode} profit={profit.StatusCode}");
            return new DashboardLoadResult
            {
                Summary = CreateEmptySummary(),
                IsMockData = false,
                ErrorMessage = "انتهت الجلسة أو غير مصرح. سجّل الدخول مرة أخرى."
            };
        }

        if (!stats.Success || stats.Data is null)
        {
            return new DashboardLoadResult
            {
                Summary = CreateEmptySummary(),
                IsMockData = false,
                ErrorMessage = DescribeFailure("إحصائيات لوحة التحكم", stats.StatusCode, stats.ErrorMessage)
            };
        }

        var summary = MapFromApi(stats.Data, sales.Data, lowStock.Data, expiring.Data, profit.Data);

        var warnings = new List<string>();
        if (!sales.Success)
        {
            warnings.Add(DescribeFailure("أحدث الفواتير", sales.StatusCode, sales.ErrorMessage));
        }

        if (!lowStock.Success)
        {
            warnings.Add(DescribeFailure("تنبيهات المخزون المنخفض", lowStock.StatusCode, lowStock.ErrorMessage));
        }

        if (!expiring.Success)
        {
            warnings.Add(DescribeFailure("تنبيهات قرب الانتهاء", expiring.StatusCode, expiring.ErrorMessage));
        }

        if (!profit.Success)
        {
            warnings.Add(DescribeFailure("أرباح اليوم", profit.StatusCode, profit.ErrorMessage));
        }

        return new DashboardLoadResult
        {
            Summary = summary,
            IsMockData = false,
            ErrorMessage = warnings.Count > 0 ? string.Join(" ", warnings) : null
        };
    }

    private static string BuildConnectionErrorMessage(
        ApiGetResult<DashboardStatsApiModel> stats,
        ApiGetResult<List<LatestSalesInvoiceApiModel>> sales,
        ApiGetResult<List<LowStockProductApiModel>> lowStock,
        ApiGetResult<List<ExpiringSoonBatchApiModel>> expiring)
    {
        var detail = stats.ErrorMessage
            ?? sales.ErrorMessage
            ?? lowStock.ErrorMessage
            ?? expiring.ErrorMessage
            ?? "تعذر الاتصال بالخادم.";
        return $"تعذر الاتصال بالخادم ({ApiConfiguration.BaseUrl}). {detail}";
    }

    private static string DescribeFailure(string section, int? statusCode, string? message)
    {
        var status = statusCode switch
        {
            404 => "الخدمة غير موجودة (404).",
            401 or 403 => "غير مصرح.",
            500 => "خطأ في الخادم (500).",
            null => string.Empty,
            _ => $"رمز الحالة {statusCode}."
        };

        var text = string.IsNullOrWhiteSpace(message) ? string.Empty : message.Trim();
        return string.IsNullOrEmpty(status)
            ? $"تعذر تحميل {section}: {text}"
            : $"تعذر تحميل {section}: {status} {text}".Trim();
    }

    private static DashboardSummary MapFromApi(
        DashboardStatsApiModel stats,
        List<LatestSalesInvoiceApiModel>? sales,
        List<LowStockProductApiModel>? lowStock,
        List<ExpiringSoonBatchApiModel>? expiring,
        DailyFinancialReportApiModel? dailyFinancial)
    {
        var latestSales = (sales ?? [])
            .Select(s => new DashboardSaleRow
            {
                InvoiceNumber = s.InvoiceNumber,
                CustomerName = string.IsNullOrWhiteSpace(s.CustomerName) ? "زبون نقدي" : s.CustomerName,
                GrandTotal = s.GrandTotal,
                Status = s.Status,
                CreatedAt = s.CreatedAt
            })
            .ToList();

        var alerts = BuildStockAlerts(lowStock, expiring);

        return new DashboardSummary
        {
            TotalProducts = stats.TotalProducts,
            TodaySales = stats.TodaySalesTotal,
            TodayProfit = dailyFinancial?.Profit?.NetProfit ?? 0m,
            LowStockProductsCount = stats.LowStockProductsCount,
            ExpiringSoonBatchesCount = stats.ExpiringSoonBatchesCount,
            TodayInvoicesCount = stats.TodayInvoicesCount,
            LatestSales = latestSales,
            StockAlerts = alerts
        };
    }

    private static List<DashboardStockAlert> BuildStockAlerts(
        List<LowStockProductApiModel>? lowStock,
        List<ExpiringSoonBatchApiModel>? expiring)
    {
        var alerts = new List<DashboardStockAlert>();

        foreach (var item in (lowStock ?? []).Take(5))
        {
            alerts.Add(new DashboardStockAlert
            {
                Title = item.ProductName,
                Detail = $"الكمية المتاحة: {item.TotalAvailableQuantity}",
                IsExpiryAlert = false,
                AlertKind = "مخزون منخفض"
            });
        }

        foreach (var batch in (expiring ?? []).Take(5))
        {
            alerts.Add(new DashboardStockAlert
            {
                Title = batch.ProductName,
                Detail = $"تنتهي {batch.ExpiryDate:yyyy-MM-dd} — الكمية: {batch.AvailableQuantity}",
                IsExpiryAlert = true,
                AlertKind = "قريب الانتهاء",
                BatchNumber = batch.BatchNumber
            });
        }

        return alerts;
    }

    private static DashboardSummary CreateEmptySummary() => new();

    private static bool IsUnauthorized(params int?[] statusCodes) =>
        statusCodes.Any(code => code == 401);
}
