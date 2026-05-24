using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Models;

internal sealed class SupplierListItemView
{
    public Guid Id { get; init; }
    public string RawName { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string Subtitle { get; init; } = string.Empty;
    public string Initials { get; init; } = "؟";
    public string ContactPerson { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public decimal TotalPurchases { get; init; }
    public decimal PayableAmount { get; init; }
    public bool HasUnpaidDues { get; init; }
    public string FormattedTotalPurchases { get; init; } = string.Empty;
    public string FormattedPayableAmount { get; init; } = string.Empty;

    public static SupplierListItemView FromApi(SupplierListItemApiModel api)
    {
        var displayName = SupplierDisplayHelper.ResolveSupplierDisplayName(api.Name);
        var payable = api.PayableAmount;
        return new SupplierListItemView
        {
            Id = api.SupplierId,
            RawName = api.Name ?? string.Empty,
            DisplayName = displayName,
            Subtitle = SupplierDisplayHelper.ResolveSupplierSubtitle(api.Name, displayName),
            Initials = SupplierDisplayHelper.ResolveInitials(displayName),
            ContactPerson = SupplierDisplayHelper.ResolveContactPerson(api.ContactPerson),
            PhoneNumber = SupplierDisplayHelper.ResolvePhone(api.Phone),
            Address = SupplierDisplayHelper.ResolveAddress(api.Address),
            TotalPurchases = api.TotalPurchases,
            PayableAmount = payable,
            HasUnpaidDues = payable > 0,
            FormattedTotalPurchases = SupplierDisplayHelper.FormatMoney(api.TotalPurchases),
            FormattedPayableAmount = payable > 0
                ? SupplierDisplayHelper.FormatMoney(payable)
                : "لا توجد مستحقات"
        };
    }

    public static SupplierListItemView FromDetails(SupplierDetailsApiModel api) =>
        FromApi(new SupplierListItemApiModel
        {
            SupplierId = api.SupplierId,
            Name = api.Name,
            ContactPerson = api.ContactPerson,
            Phone = api.Phone,
            Address = api.Address,
            TotalPurchases = api.TotalPurchases,
            PayableAmount = api.PayableAmount,
            BranchId = api.BranchId
        });
}

internal sealed class SuppliersPageState
{
    public IReadOnlyList<SupplierListItemView> Suppliers { get; init; } = Array.Empty<SupplierListItemView>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public bool IsConnectionError { get; init; }
}

internal sealed class SupplierStatsView
{
    public int TotalSuppliers { get; init; }
    public string MonthlyPurchasesText { get; init; } = "غير متوفر";
    public string UnpaidDuesText { get; init; } = "غير متوفر";
    public bool UnpaidLoaded { get; init; }
}

internal sealed class CreateSupplierResult
{
    public bool Success { get; init; }
    public SupplierListItemView? Supplier { get; init; }
    public string? ErrorMessage { get; init; }
    public bool IsConnectionError { get; init; }
}

internal sealed class UpdateSupplierResult
{
    public bool Success { get; init; }
    public SupplierListItemView? Supplier { get; init; }
    public string? ErrorMessage { get; init; }
    public bool IsConnectionError { get; init; }
}
