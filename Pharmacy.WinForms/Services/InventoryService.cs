using System.Diagnostics;
using Pharmacy.WinForms.Models;

namespace Pharmacy.WinForms.Services;

internal sealed class InventoryService
{
    private const int DefaultPageSize = 25;
    private const int MaxPageSize = 100;

    private readonly ApiClient _apiClient;

    public InventoryService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    internal static int ClampPageSize(int pageSize)
    {
        if (pageSize < 1)
        {
            return DefaultPageSize;
        }

        return Math.Min(pageSize, MaxPageSize);
    }

    public async Task<InventoryStatsView> LoadStatsAsync(CancellationToken cancellationToken = default)
    {
        _apiClient.EnsureSessionAuthorization();

        var statsTask = _apiClient.GetAsync<DashboardStatsApiModel>(
            "api/v1/dashboard/stats", "inventory/stats", cancellationToken);
        var reportTask = _apiClient.GetAsync<InventoryReportApiModel>(
            "api/v1/reports/inventory", "inventory/report", cancellationToken);

        await Task.WhenAll(statsTask, reportTask).ConfigureAwait(false);

        var stats = await statsTask.ConfigureAwait(false);
        var report = await reportTask.ConfigureAwait(false);

        if (stats.IsConnectionError || report.IsConnectionError)
        {
            return InventoryStatsView.Empty(
                stats.ErrorMessage ?? report.ErrorMessage ?? "تعذر الاتصال بالخادم.");
        }

        if (!stats.Success || stats.Data is null)
        {
            return InventoryStatsView.Empty(stats.ErrorMessage ?? "تعذر تحميل إحصائيات المخزون.");
        }

        var reportData = report.Success ? report.Data : null;
        var total = stats.Data.TotalProducts;
        var lowStock = stats.Data.LowStockProductsCount;
        var active = Math.Max(0, total - lowStock);
        var expiringOrExpired = (reportData?.ExpiringSoonBatchesCount ?? stats.Data.ExpiringSoonBatchesCount)
            + (reportData?.ExpiredBatchesCount ?? 0);

        string? badge = null;
        if (total > 0)
        {
            var pct = (int)Math.Round(active * 100.0 / total);
            badge = $"{pct}%";
        }

        return new InventoryStatsView
        {
            HasData = true,
            TotalProducts = total,
            ActiveAvailable = active,
            LowStockCount = lowStock,
            ExpiringOrExpiredCount = expiringOrExpired,
            ActiveAvailableBadge = badge
        };
    }

    public async Task<InventoryPageState> LoadProductsPageAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        _apiClient.EnsureSessionAuthorization();

        var page = Math.Max(1, pageNumber);
        var size = ClampPageSize(pageSize);
        var url =
            $"api/v1/products?pageNumber={page}&pageSize={size}&sortBy=name&sortDirection=asc";

        var expiringIds = await LoadExpiringProductIdsAsync(cancellationToken).ConfigureAwait(false);
        var result = await _apiClient.GetAsync<PagedInventoryProductsApiModel>(
            url, "inventory/products", cancellationToken).ConfigureAwait(false);

        if (!result.Success)
        {
            return new InventoryPageState
            {
                Success = false,
                ErrorMessage = result.ErrorMessage ?? "تعذر تحميل المخزون.",
                IsConnectionError = result.IsConnectionError,
                PageNumber = page,
                PageSize = size
            };
        }

        if (result.Data is null)
        {
            return new InventoryPageState
            {
                Success = false,
                ErrorMessage = "استجابة غير صالحة من الخادم.",
                PageNumber = page,
                PageSize = size
            };
        }

        var products = result.Data.Items
            .Select(p => InventoryProductView.FromApi(p, expiringIds.Contains(p.ProductId)))
            .ToList();

        return new InventoryPageState
        {
            Success = true,
            Products = products,
            TotalCount = result.Data.TotalCount,
            PageNumber = page,
            PageSize = size
        };
    }

    public async Task<InventoryPageState> SearchProductsAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return await LoadProductsPageAsync(1, DefaultPageSize, cancellationToken).ConfigureAwait(false);
        }

        _apiClient.EnsureSessionAuthorization();

        var encoded = Uri.EscapeDataString(query.Trim());
        var expiringIds = await LoadExpiringProductIdsAsync(cancellationToken).ConfigureAwait(false);
        var result = await _apiClient.GetAsync<List<InventoryProductApiModel>>(
            $"api/v1/products/search?query={encoded}",
            "inventory/search",
            cancellationToken).ConfigureAwait(false);

        if (!result.Success)
        {
            return new InventoryPageState
            {
                Success = false,
                ErrorMessage = result.ErrorMessage ?? "تعذر البحث عن المنتجات.",
                IsConnectionError = result.IsConnectionError
            };
        }

        var products = (result.Data ?? [])
            .Select(p => InventoryProductView.FromApi(p, expiringIds.Contains(p.ProductId)))
            .ToList();

        return new InventoryPageState
        {
            Success = true,
            Products = products,
            TotalCount = products.Count,
            PageNumber = 1,
            PageSize = products.Count > 0 ? products.Count : DefaultPageSize
        };
    }

    public async Task<InventoryProductDetailsView?> LoadProductDetailsAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        _apiClient.EnsureSessionAuthorization();

        var productTask = _apiClient.GetAsync<InventoryProductDetailsApiModel>(
            $"api/v1/products/{productId}",
            "inventory/product-details",
            cancellationToken);
        var batchesTask = _apiClient.GetAsync<List<InventoryStockBatchApiModel>>(
            $"api/v1/stock-batches/by-product/{productId}",
            "inventory/batches",
            cancellationToken);
        var transactionsTask = _apiClient.GetAsync<List<InventoryTransactionApiModel>>(
            "api/v1/inventory-transactions",
            "inventory/transactions",
            cancellationToken);

        await Task.WhenAll(productTask, batchesTask, transactionsTask).ConfigureAwait(false);

        var productResult = await productTask.ConfigureAwait(false);
        if (!productResult.Success || productResult.Data is null)
        {
            Debug.WriteLine($"[Inventory] Product details failed: {productResult.ErrorMessage}");
            return null;
        }

        var batches = (await batchesTask.ConfigureAwait(false)).Data ?? [];
        var allTransactions = (await transactionsTask.ConfigureAwait(false)).Data ?? [];
        var expiringSoon = batches
            .Where(b => b.AvailableQuantity > 0 && b.ExpiryDate <= DateTime.UtcNow.AddDays(90))
            .OrderBy(b => b.ExpiryDate)
            .ToList();

        var isExpiringSoon = expiringSoon.Count > 0;
        var productView = InventoryProductView.FromDetailsApi(productResult.Data, isExpiringSoon);
        var supplierName = batches
            .Select(b => b.SupplierName)
            .FirstOrDefault(n => !string.IsNullOrWhiteSpace(n)) ?? "غير متوفر";

        DateTime? nearestExpiry = expiringSoon.FirstOrDefault()?.ExpiryDate;
        string? expiryWarning = null;
        if (nearestExpiry.HasValue)
        {
            expiryWarning = $"الدفعة الحالية تنتهي في {nearestExpiry.Value:MM/yyyy}، يرجى تصريفها بأقرب وقت.";
        }
        else if (productView.ExpiredQuantity > 0)
        {
            expiryWarning = "يوجد مخزون منتهي الصلاحية، يرجى مراجعته.";
        }

        var transactions = allTransactions
            .Where(t => t.ProductId == productId)
            .OrderByDescending(t => t.CreatedAt)
            .Take(20)
            .Select(InventoryTransactionView.FromApi)
            .ToList();

        return new InventoryProductDetailsView
        {
            Product = productView,
            SupplierName = supplierName,
            NearestExpiryDate = nearestExpiry,
            ExpiryWarningText = expiryWarning,
            Transactions = transactions
        };
    }

    public async Task<IReadOnlyList<InventoryProductView>> LoadLowStockProductsAsync(
        CancellationToken cancellationToken = default)
    {
        _apiClient.EnsureSessionAuthorization();

        var result = await _apiClient.GetAsync<List<InventoryLowStockProductApiModel>>(
            "api/v1/dashboard/low-stock-products",
            "inventory/low-stock",
            cancellationToken).ConfigureAwait(false);

        if (!result.Success || result.Data is null)
        {
            return Array.Empty<InventoryProductView>();
        }

        var views = new List<InventoryProductView>();
        foreach (var item in result.Data)
        {
            var details = await _apiClient.GetAsync<InventoryProductDetailsApiModel>(
                $"api/v1/products/{item.ProductId}",
                "inventory/low-stock-product",
                cancellationToken).ConfigureAwait(false);

            if (details.Success && details.Data is not null)
            {
                views.Add(InventoryProductView.FromDetailsApi(details.Data));
            }
        }

        return views;
    }

    public async Task<IReadOnlyList<InventoryProductView>> LoadExpiringProductsAsync(
        CancellationToken cancellationToken = default)
    {
        _apiClient.EnsureSessionAuthorization();

        var result = await _apiClient.GetAsync<List<InventoryStockBatchApiModel>>(
            "api/v1/stock-batches/expiring-soon",
            "inventory/expiring",
            cancellationToken).ConfigureAwait(false);

        if (!result.Success || result.Data is null)
        {
            return Array.Empty<InventoryProductView>();
        }

        var productIds = result.Data.Select(b => b.ProductId).Distinct().ToList();
        var views = new List<InventoryProductView>();

        foreach (var productId in productIds)
        {
            var details = await _apiClient.GetAsync<InventoryProductDetailsApiModel>(
                $"api/v1/products/{productId}",
                "inventory/expiring-product",
                cancellationToken).ConfigureAwait(false);

            if (details.Success && details.Data is not null)
            {
                views.Add(InventoryProductView.FromDetailsApi(details.Data, isExpiringSoon: true));
            }
        }

        return views;
    }

    private async Task<HashSet<Guid>> LoadExpiringProductIdsAsync(CancellationToken cancellationToken)
    {
        var result = await _apiClient.GetAsync<List<InventoryStockBatchApiModel>>(
            "api/v1/stock-batches/expiring-soon",
            "inventory/expiring-ids",
            cancellationToken).ConfigureAwait(false);

        if (!result.Success || result.Data is null)
        {
            return [];
        }

        return result.Data.Select(b => b.ProductId).ToHashSet();
    }
}
