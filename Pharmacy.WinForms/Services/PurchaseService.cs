using Pharmacy.WinForms.Models;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Services;

internal sealed class PurchaseService
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    private readonly ApiClient _apiClient;

    public PurchaseService(ApiClient apiClient)
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

    public async Task<PurchaseInvoicesPageState> LoadInvoicesPageAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        _apiClient.EnsureSessionAuthorization();

        var page = Math.Max(1, pageNumber);
        var size = ClampPageSize(pageSize);
        var url =
            $"api/v1/purchase-invoices?pageNumber={page}&pageSize={size}&sortBy=createdat&sortDirection=desc";

        var result = await _apiClient.GetAsync<PagedPurchaseInvoicesApiModel>(
            url,
            "purchases/invoices",
            cancellationToken).ConfigureAwait(false);

        if (!result.Success)
        {
            return new PurchaseInvoicesPageState
            {
                Success = false,
                ErrorMessage = result.ErrorMessage ?? "تعذر تحميل فواتير الشراء.",
                IsConnectionError = result.IsConnectionError,
                PageNumber = page,
                PageSize = size
            };
        }

        if (result.Data is null)
        {
            return new PurchaseInvoicesPageState
            {
                Success = false,
                ErrorMessage = "استجابة غير صالحة من الخادم.",
                PageNumber = page,
                PageSize = size
            };
        }

        var invoices = result.Data.Items
            .Select(p => PurchaseInvoiceListItemView.FromApi(p))
            .ToList();

        return new PurchaseInvoicesPageState
        {
            Success = true,
            Invoices = invoices,
            TotalCount = result.Data.TotalCount,
            PageNumber = page,
            PageSize = size
        };
    }

    public async Task<PurchaseInvoiceDetailsView?> LoadInvoiceDetailsAsync(
        Guid invoiceId,
        CancellationToken cancellationToken = default)
    {
        _apiClient.EnsureSessionAuthorization();

        var detailsTask = _apiClient.GetAsync<PurchaseInvoiceDetailsApiModel>(
            $"api/v1/purchase-invoices/{invoiceId}",
            "purchases/invoice-details",
            cancellationToken);
        var itemsTask = _apiClient.GetAsync<List<PurchaseInvoiceItemApiModel>>(
            $"api/v1/purchase-invoices/{invoiceId}/items",
            "purchases/invoice-items",
            cancellationToken);

        await Task.WhenAll(detailsTask, itemsTask).ConfigureAwait(false);

        var details = await detailsTask.ConfigureAwait(false);
        if (!details.Success || details.Data is null)
        {
            return null;
        }

        var items = (await itemsTask.ConfigureAwait(false)).Data ?? [];
        var summary = PurchaseInvoiceListItemView.FromDetails(details.Data, items.Count);
        var lines = items.Select(PurchaseInvoiceLineView.FromApi).ToList();

        return new PurchaseInvoiceDetailsView
        {
            Summary = summary,
            Lines = lines
        };
    }

    public async Task<IReadOnlyList<SupplierOptionView>> LoadSuppliersAsync(
        CancellationToken cancellationToken = default)
    {
        _apiClient.EnsureSessionAuthorization();

        var options = new List<SupplierOptionView> { SupplierOptionView.All };
        var page = 1;
        var total = int.MaxValue;
        var collected = new List<SupplierListApiModel>();

        while (collected.Count < total && page <= 10)
        {
            var result = await _apiClient.GetAsync<PagedSuppliersApiModel>(
                $"api/v1/suppliers?pageNumber={page}&pageSize={50}&sortBy=name&sortDirection=asc",
                "purchases/suppliers",
                cancellationToken).ConfigureAwait(false);

            if (!result.Success || result.Data is null)
            {
                break;
            }

            total = result.Data.TotalCount;
            if (result.Data.Items.Count == 0)
            {
                break;
            }

            collected.AddRange(result.Data.Items);
            page++;
        }

        options.AddRange(collected
            .Where(s => !string.IsNullOrWhiteSpace(s.Name))
            .OrderBy(s => s.Name, StringComparer.CurrentCultureIgnoreCase)
            .Select(s =>
            {
                var raw = s.Name.Trim();
                var display = PurchaseDisplayHelper.ResolveSupplierDisplayName(raw);
                return new SupplierOptionView
                {
                    SupplierId = s.SupplierId,
                    Name = raw,
                    DisplayName = display,
                    Subtitle = PurchaseDisplayHelper.ResolveSupplierSubtitle(raw, display)
                };
            }));

        return options;
    }

    public async Task<IReadOnlyList<SupplierOptionView>> LoadSupplierChoicesAsync(
        CancellationToken cancellationToken = default)
    {
        var suppliers = await LoadSuppliersAsync(cancellationToken).ConfigureAwait(false);
        return suppliers.Where(s => s.SupplierId.HasValue).ToList();
    }

    public async Task<ProductsLoadResult> LoadProductsAsync(CancellationToken cancellationToken = default)
    {
        _apiClient.EnsureSessionAuthorization();

        var pageSize = PointOfSaleService.ClampPageSize(100);
        var all = new List<PosProductApiModel>();
        var page = 1;
        var total = int.MaxValue;

        while (all.Count < total && page <= 20)
        {
            var url = $"api/v1/products?pageNumber={page}&pageSize={pageSize}&sortBy=name&sortDirection=asc";
            var result = await _apiClient.GetAsync<PagedProductsApiModel>(
                url,
                "purchases/products",
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

            all.AddRange(result.Data.Items);
            page++;
        }

        var views = all
            .Select(PosProductView.FromApi)
            .OrderBy(p => p.DisplayName, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        return new ProductsLoadResult
        {
            Success = true,
            Products = views
        };
    }

    public async Task<CreatePurchaseInvoiceResult> CreatePurchaseInvoiceAsync(
        CreatePurchaseInvoiceApiRequest request,
        CancellationToken cancellationToken = default)
    {
        _apiClient.EnsureSessionAuthorization();

        var result = await _apiClient.PostAsync<CreatePurchaseInvoiceApiRequest, PurchaseInvoiceDetailsApiModel>(
            "api/v1/purchase-invoices",
            request,
            "purchases/create-invoice",
            cancellationToken).ConfigureAwait(false);

        if (!result.Success)
        {
            return new CreatePurchaseInvoiceResult
            {
                Success = false,
                ErrorMessage = result.ErrorMessage ?? "تعذر حفظ فاتورة الشراء.",
                IsConnectionError = result.IsConnectionError
            };
        }

        return new CreatePurchaseInvoiceResult
        {
            Success = true,
            Invoice = result.Data
        };
    }
}

internal sealed class CreatePurchaseInvoiceResult
{
    public bool Success { get; init; }
    public PurchaseInvoiceDetailsApiModel? Invoice { get; init; }
    public string? ErrorMessage { get; init; }
    public bool IsConnectionError { get; init; }
}
