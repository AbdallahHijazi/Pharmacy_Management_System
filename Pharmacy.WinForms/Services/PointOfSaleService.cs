using System.Diagnostics;
using Pharmacy.WinForms.Models;

namespace Pharmacy.WinForms.Services;

internal sealed class PointOfSaleService
{
    private const int DefaultPageSize = 50;
    private const int MaxPageSize = 100;

    private readonly ApiClient _apiClient;

    public PointOfSaleService(ApiClient apiClient)
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

    public async Task<ProductsLoadResult> LoadProductsAsync(CancellationToken cancellationToken = default)
    {
        _apiClient.EnsureSessionAuthorization();

        var pageSize = ClampPageSize(DefaultPageSize);
        var all = new List<PosProductApiModel>();
        var page = 1;
        var total = int.MaxValue;

        while (all.Count < total)
        {
            var url =
                $"api/v1/products?pageNumber={page}&pageSize={pageSize}&sortBy=name&sortDirection=asc";
            var result = await _apiClient.GetAsync<PagedProductsApiModel>(
                url,
                "pos/products",
                cancellationToken).ConfigureAwait(false);

            if (!result.Success)
            {
                return new ProductsLoadResult
                {
                    Success = false,
                    ErrorMessage = result.ErrorMessage ?? "تعذر تحميل المنتجات.",
                    IsConnectionError = result.IsConnectionError
                };
            }

            if (result.Data is null)
            {
                return new ProductsLoadResult
                {
                    Success = false,
                    ErrorMessage = "استجابة غير صالحة من الخادم."
                };
            }

            total = result.Data.TotalCount;
            if (result.Data.Items.Count == 0)
            {
                break;
            }

            if (all.Count == 0)
            {
                LogFirstProductRaw(result.Data.Items[0]);
            }

            all.AddRange(result.Data.Items);
            page++;

            if (page > 50)
            {
                break;
            }
        }

        var views = all
            .Select(PosProductView.FromApi)
            .OrderBy(p => p.DisplayName, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        if (views.Count > 0)
        {
            var mapped = views[0];
            Debug.WriteLine(
                $"[POS] First product mapped: ProductId={mapped.ProductId}, " +
                $"rawName='{mapped.Name}', IsGeneratedTestName={PosProductView.IsGeneratedTestName(mapped.Name)}, " +
                $"DisplayName='{mapped.DisplayName}', Subtitle='{mapped.Subtitle}', " +
                $"ScientificName='{mapped.ScientificName}', Barcode='{mapped.Barcode}'");
        }

        return new ProductsLoadResult
        {
            Success = true,
            Products = views
        };
    }

    public async Task<ProductsLoadResult> SearchProductsAsync(string query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return new ProductsLoadResult { Success = true, Products = Array.Empty<PosProductView>() };
        }

        _apiClient.EnsureSessionAuthorization();
        var encoded = Uri.EscapeDataString(query.Trim());
        var result = await _apiClient.GetAsync<List<PosProductApiModel>>(
            $"api/v1/products/search?query={encoded}",
            "pos/search",
            cancellationToken).ConfigureAwait(false);

        if (!result.Success)
        {
            return new ProductsLoadResult
            {
                Success = false,
                ErrorMessage = result.ErrorMessage ?? "تعذر البحث عن المنتجات.",
                IsConnectionError = result.IsConnectionError
            };
        }

        var views = (result.Data ?? [])
            .Select(PosProductView.FromApi)
            .ToList();

        return new ProductsLoadResult
        {
            Success = true,
            Products = views
        };
    }

    public async Task<CreateInvoiceResult> CreateSalesInvoiceAsync(
        CreateSalesInvoiceApiRequest request,
        CancellationToken cancellationToken = default)
    {
        _apiClient.EnsureSessionAuthorization();
        var result = await _apiClient.PostAsync<CreateSalesInvoiceApiRequest, SalesInvoiceCreatedApiModel>(
            "api/v1/sales-invoices",
            request,
            "pos/create-invoice",
            cancellationToken).ConfigureAwait(false);

        if (!result.Success)
        {
            return new CreateInvoiceResult
            {
                Success = false,
                ErrorMessage = result.ErrorMessage ?? "تعذر إنشاء الفاتورة.",
                IsConnectionError = result.IsConnectionError
            };
        }

        return new CreateInvoiceResult
        {
            Success = true,
            Invoice = result.Data
        };
    }

    private static void LogFirstProductRaw(PosProductApiModel api)
    {
        Debug.WriteLine(
            $"[POS] First product raw fields: ProductId={api.ProductId}, " +
            $"Name='{api.Name}', ProductName='{api.ProductName}', TradeName='{api.TradeName}', " +
            $"CommercialName='{api.CommercialName}', ArabicName='{api.ArabicName}', ScientificName='{api.ScientificName}', " +
            $"Barcode='{api.Barcode}', Sku='{api.Sku}', Code='{api.Code}', " +
            $"CategoryName='{api.CategoryName}'");
    }
}

internal sealed class ProductsLoadResult
{
    public bool Success { get; init; }
    public IReadOnlyList<PosProductView> Products { get; init; } = Array.Empty<PosProductView>();
    public string? ErrorMessage { get; init; }
    public bool IsConnectionError { get; init; }
}

internal sealed class CreateInvoiceResult
{
    public bool Success { get; init; }
    public SalesInvoiceCreatedApiModel? Invoice { get; init; }
    public string? ErrorMessage { get; init; }
    public bool IsConnectionError { get; init; }
}
