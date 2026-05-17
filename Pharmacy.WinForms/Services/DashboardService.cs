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
        _apiClient.SetBearerToken(SessionManager.Token);

        var today = DateTime.UtcNow.Date;
        var dateQuery = today.ToString("yyyy-MM-dd");

        var statsTask = _apiClient.GetAsync<DashboardStatsApiModel>("api/v1/dashboard/stats", cancellationToken);
        var salesTask = _apiClient.GetAsync<List<LatestSalesInvoiceApiModel>>(
            "api/v1/dashboard/latest-sales-invoices",
            cancellationToken);
        var lowStockTask = _apiClient.GetAsync<List<LowStockProductApiModel>>(
            "api/v1/dashboard/low-stock-products",
            cancellationToken);
        var expiringTask = _apiClient.GetAsync<List<ExpiringSoonBatchApiModel>>(
            "api/v1/dashboard/expiring-soon-batches",
            cancellationToken);
        var profitTask = _apiClient.GetAsync<DailyFinancialReportApiModel>(
            $"api/v1/reports/financial/daily?date={dateQuery}",
            cancellationToken);

        await Task.WhenAll(statsTask, salesTask, lowStockTask, expiringTask, profitTask).ConfigureAwait(false);

        var stats = await statsTask.ConfigureAwait(false);
        var sales = await salesTask.ConfigureAwait(false);
        var lowStock = await lowStockTask.ConfigureAwait(false);
        var expiring = await expiringTask.ConfigureAwait(false);
        var profit = await profitTask.ConfigureAwait(false);

        var connectionFailed = stats.IsConnectionError
            || sales.IsConnectionError
            || lowStock.IsConnectionError
            || expiring.IsConnectionError;

        if (connectionFailed && !stats.Success)
        {
            return new DashboardLoadResult
            {
                Summary = CreateMockSummary(),
                IsMockData = true,
                ErrorMessage = stats.ErrorMessage
                    ?? sales.ErrorMessage
                    ?? "تعذر الاتصال بالخادم. يتم عرض بيانات تجريبية."
            };
        }

        var summary = stats.Success && stats.Data is not null
            ? MapFromApi(stats.Data, sales.Data, lowStock.Data, expiring.Data, profit.Data)
            : CreateMockSummary();

        var hasPartialError = !stats.Success
            || !sales.Success
            || !lowStock.Success
            || !expiring.Success;

        return new DashboardLoadResult
        {
            Summary = summary,
            IsMockData = !stats.Success,
            ErrorMessage = hasPartialError
                ? "تعذر تحميل بعض بيانات لوحة التحكم. تم عرض المتاح أو بيانات تجريبية."
                : null
        };
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
                Detail = $"مخزون منخفض — الكمية المتاحة: {item.TotalAvailableQuantity}",
                IsExpiryAlert = false
            });
        }

        foreach (var batch in (expiring ?? []).Take(5))
        {
            alerts.Add(new DashboardStockAlert
            {
                Title = batch.ProductName,
                Detail = $"دفعة {batch.BatchNumber} — تنتهي {batch.ExpiryDate:yyyy-MM-dd} — الكمية: {batch.AvailableQuantity}",
                IsExpiryAlert = true
            });
        }

        return alerts;
    }

    private static DashboardSummary CreateMockSummary() => new()
    {
        TotalProducts = 248,
        TodaySales = 3_450.75m,
        TodayProfit = 820.40m,
        LowStockProductsCount = 12,
        ExpiringSoonBatchesCount = 7,
        TodayInvoicesCount = 18,
        LatestSales =
        [
            new DashboardSaleRow
            {
                InvoiceNumber = "INV-1042",
                CustomerName = "أحمد خليل",
                GrandTotal = 185.50m,
                Status = "Paid",
                CreatedAt = DateTime.Now.AddHours(-1)
            },
            new DashboardSaleRow
            {
                InvoiceNumber = "INV-1041",
                CustomerName = "زبون نقدي",
                GrandTotal = 42.00m,
                Status = "Paid",
                CreatedAt = DateTime.Now.AddHours(-3)
            },
            new DashboardSaleRow
            {
                InvoiceNumber = "INV-1040",
                CustomerName = "مريم سعيد",
                GrandTotal = 310.25m,
                Status = "Paid",
                CreatedAt = DateTime.Now.AddHours(-5)
            }
        ],
        StockAlerts =
        [
            new DashboardStockAlert
            {
                Title = "باراسيتامول 500mg",
                Detail = "مخزون منخفض — الكمية المتاحة: 8",
                IsExpiryAlert = false
            },
            new DashboardStockAlert
            {
                Title = "أموكسيسيلين 250mg",
                Detail = "دفعة B-2201 — تنتهي 2026-06-15 — الكمية: 24",
                IsExpiryAlert = true
            }
        ]
    };
}
