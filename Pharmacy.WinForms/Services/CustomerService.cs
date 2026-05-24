using Pharmacy.WinForms.Models;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Services;

internal sealed class CustomerService
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    private readonly ApiClient _apiClient;

    public CustomerService(ApiClient apiClient)
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

    public async Task<CustomersPageState> LoadCustomersPageAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        _apiClient.EnsureSessionAuthorization();

        var page = Math.Max(1, pageNumber);
        var size = ClampPageSize(pageSize);
        var url =
            $"api/v1/customers?pageNumber={page}&pageSize={size}&sortBy=fullname&sortDirection=asc";

        var result = await _apiClient.GetAsync<PagedCustomersApiModel>(
            url,
            "customers/list",
            cancellationToken).ConfigureAwait(false);

        if (!result.Success)
        {
            return new CustomersPageState
            {
                Success = false,
                ErrorMessage = result.ErrorMessage ?? "تعذر تحميل الزبائن.",
                IsConnectionError = result.IsConnectionError,
                PageNumber = page,
                PageSize = size
            };
        }

        if (result.Data is null)
        {
            return new CustomersPageState
            {
                Success = false,
                ErrorMessage = "استجابة غير صالحة من الخادم.",
                PageNumber = page,
                PageSize = size
            };
        }

        var customers = result.Data.Items.Select(CustomerListItemView.FromApi).ToList();
        return new CustomersPageState
        {
            Success = true,
            Customers = customers,
            TotalCount = result.Data.TotalCount,
            PageNumber = page,
            PageSize = size
        };
    }

    public async Task<IReadOnlyList<CustomerListItemView>> SearchCustomersAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        _apiClient.EnsureSessionAuthorization();

        if (string.IsNullOrWhiteSpace(query))
        {
            return Array.Empty<CustomerListItemView>();
        }

        var encoded = Uri.EscapeDataString(query.Trim());
        var result = await _apiClient.GetAsync<List<CustomerListItemApiModel>>(
            $"api/v1/customers/search?query={encoded}",
            "customers/search",
            cancellationToken).ConfigureAwait(false);

        if (!result.Success || result.Data is null)
        {
            return Array.Empty<CustomerListItemView>();
        }

        return result.Data.Select(CustomerListItemView.FromApi).ToList();
    }

    public async Task<CustomerListItemView?> LoadCustomerDetailsAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        _apiClient.EnsureSessionAuthorization();

        var result = await _apiClient.GetAsync<CustomerDetailsApiModel>(
            $"api/v1/customers/{customerId}",
            "customers/details",
            cancellationToken).ConfigureAwait(false);

        return result.Success && result.Data is not null
            ? CustomerListItemView.FromDetails(result.Data)
            : null;
    }

    public async Task<CreateCustomerResult> CreateCustomerAsync(
        CreateCustomerApiRequest request,
        CancellationToken cancellationToken = default)
    {
        _apiClient.EnsureSessionAuthorization();

        var result = await _apiClient.PostAsync<CreateCustomerApiRequest, CustomerDetailsApiModel>(
            "api/v1/customers",
            request,
            "customers/create",
            cancellationToken).ConfigureAwait(false);

        if (!result.Success)
        {
            return new CreateCustomerResult
            {
                Success = false,
                ErrorMessage = result.ErrorMessage ?? "تعذر إضافة الزبون.",
                IsConnectionError = result.IsConnectionError
            };
        }

        if (result.Data is null)
        {
            return new CreateCustomerResult
            {
                Success = false,
                ErrorMessage = "استجابة غير صالحة من الخادم."
            };
        }

        return new CreateCustomerResult
        {
            Success = true,
            Customer = CustomerListItemView.FromDetails(result.Data)
        };
    }
}
