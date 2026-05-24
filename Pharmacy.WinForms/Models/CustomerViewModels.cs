using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Models;

internal enum CustomerViewMode
{
    Grid,
    List
}

internal sealed class CustomerListItemView
{
    public Guid Id { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string Initials { get; init; } = "؟";
    public decimal TotalPurchases { get; init; }
    public decimal DebtAmount { get; init; }
    public bool HasDebt { get; init; }
    public bool IsOverdue { get; init; }
    public string DebtStatusText { get; init; } = string.Empty;
    public string FormattedTotalPurchases { get; init; } = string.Empty;

    public static CustomerListItemView FromApi(CustomerListItemApiModel api)
    {
        var displayName = CustomerDisplayHelper.ResolveDisplayName(api.FullName);
        var debt = api.DebtAmount;
        return new CustomerListItemView
        {
            Id = api.CustomerId,
            DisplayName = displayName,
            PhoneNumber = CustomerDisplayHelper.ResolvePhone(api.Phone),
            Address = CustomerDisplayHelper.ResolveAddress(api.Address),
            Initials = CustomerDisplayHelper.ResolveInitials(displayName),
            TotalPurchases = api.TotalPurchases,
            DebtAmount = debt,
            HasDebt = debt > 0,
            IsOverdue = debt > 0,
            DebtStatusText = CustomerDisplayHelper.ResolveDebtStatus(debt),
            FormattedTotalPurchases = PosFormatting.FormatMoneyCompact(api.TotalPurchases)
        };
    }

    public static CustomerListItemView FromDetails(CustomerDetailsApiModel api) =>
        FromApi(new CustomerListItemApiModel
        {
            CustomerId = api.CustomerId,
            FullName = api.FullName,
            Phone = api.Phone,
            Address = api.Address,
            TotalPurchases = api.TotalPurchases,
            DebtAmount = api.DebtAmount,
            BranchId = api.BranchId
        });
}

internal sealed class CustomersPageState
{
    public IReadOnlyList<CustomerListItemView> Customers { get; init; } = Array.Empty<CustomerListItemView>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public bool IsConnectionError { get; init; }
}

internal sealed class CreateCustomerResult
{
    public bool Success { get; init; }
    public CustomerListItemView? Customer { get; init; }
    public string? ErrorMessage { get; init; }
    public bool IsConnectionError { get; init; }
}
