using Pharmacy.WinForms.Models;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Services;

internal sealed class SupplierService
{
    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 100;
    private const int StatsPageSize = 50;
    private const int MaxStatsPages = 20;

    private readonly ApiClient _apiClient;

    public SupplierService(ApiClient apiClient)
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

    public async Task<SuppliersPageState> LoadSuppliersPageAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        _apiClient.EnsureSessionAuthorization();

        var page = Math.Max(1, pageNumber);
        var size = ClampPageSize(pageSize);
        var url =
            $"api/v1/suppliers?pageNumber={page}&pageSize={size}&sortBy=name&sortDirection=asc";

        var result = await _apiClient.GetAsync<PagedSupplierItemsApiModel>(
            url,
            "suppliers/list",
            cancellationToken).ConfigureAwait(false);

        if (!result.Success)
        {
            return new SuppliersPageState
            {
                Success = false,
                ErrorMessage = result.ErrorMessage ?? "تعذر تحميل الموردين.",
                IsConnectionError = result.IsConnectionError,
                PageNumber = page,
                PageSize = size
            };
        }

        if (result.Data is null)
        {
            return new SuppliersPageState
            {
                Success = false,
                ErrorMessage = "استجابة غير صالحة من الخادم.",
                PageNumber = page,
                PageSize = size
            };
        }

        var suppliers = result.Data.Items.Select(SupplierListItemView.FromApi).ToList();
        return new SuppliersPageState
        {
            Success = true,
            Suppliers = suppliers,
            TotalCount = result.Data.TotalCount,
            PageNumber = page,
            PageSize = size
        };
    }

    public async Task<IReadOnlyList<SupplierListItemView>> SearchSuppliersAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        _apiClient.EnsureSessionAuthorization();

        if (string.IsNullOrWhiteSpace(query))
        {
            return Array.Empty<SupplierListItemView>();
        }

        var encoded = Uri.EscapeDataString(query.Trim());
        var result = await _apiClient.GetAsync<List<SupplierListItemApiModel>>(
            $"api/v1/suppliers/search?query={encoded}",
            "suppliers/search",
            cancellationToken).ConfigureAwait(false);

        if (!result.Success || result.Data is null)
        {
            return Array.Empty<SupplierListItemView>();
        }

        return result.Data.Select(SupplierListItemView.FromApi).ToList();
    }

    public async Task<SupplierListItemView?> LoadSupplierDetailsAsync(
        Guid supplierId,
        CancellationToken cancellationToken = default)
    {
        _apiClient.EnsureSessionAuthorization();

        var result = await _apiClient.GetAsync<SupplierDetailsApiModel>(
            $"api/v1/suppliers/{supplierId}",
            "suppliers/details",
            cancellationToken).ConfigureAwait(false);

        return result.Success && result.Data is not null
            ? SupplierListItemView.FromDetails(result.Data)
            : null;
    }

    public async Task<SupplierStatsView> LoadSupplierStatsAsync(
        CancellationToken cancellationToken = default)
    {
        _apiClient.EnsureSessionAuthorization();

        var firstPage = await LoadSuppliersPageAsync(1, StatsPageSize, cancellationToken).ConfigureAwait(false);
        if (!firstPage.Success)
        {
            return new SupplierStatsView();
        }

        var totalSuppliers = firstPage.TotalCount;
        decimal unpaidTotal = firstPage.Suppliers.Sum(s => s.PayableAmount);
        var loadedPages = 1;
        var page = 2;

        while (loadedPages < MaxStatsPages && firstPage.Suppliers.Count > 0)
        {
            var expectedLoaded = loadedPages * StatsPageSize;
            if (expectedLoaded >= totalSuppliers)
            {
                break;
            }

            var nextPage = await LoadSuppliersPageAsync(page, StatsPageSize, cancellationToken).ConfigureAwait(false);
            if (!nextPage.Success || nextPage.Suppliers.Count == 0)
            {
                break;
            }

            unpaidTotal += nextPage.Suppliers.Sum(s => s.PayableAmount);
            loadedPages++;
            page++;
        }

        var unpaidLoaded = loadedPages * StatsPageSize >= totalSuppliers || totalSuppliers == 0;
        return new SupplierStatsView
        {
            TotalSuppliers = totalSuppliers,
            MonthlyPurchasesText = "غير متوفر",
            UnpaidDuesText = unpaidLoaded
                ? SupplierDisplayHelper.FormatMoney(unpaidTotal)
                : "غير متوفر",
            UnpaidLoaded = unpaidLoaded
        };
    }

    public async Task<CreateSupplierResult> CreateSupplierAsync(
        CreateSupplierApiRequest request,
        CancellationToken cancellationToken = default)
    {
        _apiClient.EnsureSessionAuthorization();

        var result = await _apiClient.PostAsync<CreateSupplierApiRequest, SupplierDetailsApiModel>(
            "api/v1/suppliers",
            request,
            "suppliers/create",
            cancellationToken).ConfigureAwait(false);

        if (!result.Success)
        {
            return new CreateSupplierResult
            {
                Success = false,
                ErrorMessage = result.ErrorMessage ?? "تعذر إضافة المورد.",
                IsConnectionError = result.IsConnectionError
            };
        }

        if (result.Data is null)
        {
            return new CreateSupplierResult
            {
                Success = false,
                ErrorMessage = "استجابة غير صالحة من الخادم."
            };
        }

        return new CreateSupplierResult
        {
            Success = true,
            Supplier = SupplierListItemView.FromDetails(result.Data)
        };
    }

    public async Task<UpdateSupplierResult> UpdateSupplierAsync(
        Guid supplierId,
        UpdateSupplierApiRequest request,
        CancellationToken cancellationToken = default)
    {
        _apiClient.EnsureSessionAuthorization();

        var result = await _apiClient.PutAsync<UpdateSupplierApiRequest>(
            $"api/v1/suppliers/{supplierId}",
            request,
            "suppliers/update",
            cancellationToken).ConfigureAwait(false);

        if (!result.Success)
        {
            return new UpdateSupplierResult
            {
                Success = false,
                ErrorMessage = result.ErrorMessage ?? "تعذر تحديث المورد.",
                IsConnectionError = result.IsConnectionError
            };
        }

        var refreshed = await LoadSupplierDetailsAsync(supplierId, cancellationToken).ConfigureAwait(false);
        if (refreshed is null)
        {
            return new UpdateSupplierResult
            {
                Success = false,
                ErrorMessage = "تم التحديث لكن تعذر تحميل بيانات المورد."
            };
        }

        return new UpdateSupplierResult
        {
            Success = true,
            Supplier = refreshed
        };
    }
}
