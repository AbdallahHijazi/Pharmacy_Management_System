namespace Pharmacy.WinForms.Models;

internal sealed class PagedCustomersApiModel
{
    public List<CustomerListItemApiModel> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

internal sealed class CustomerListItemApiModel
{
    public Guid CustomerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public decimal TotalPurchases { get; set; }
    public decimal DebtAmount { get; set; }
    public Guid BranchId { get; set; }
}

internal sealed class CustomerDetailsApiModel
{
    public Guid CustomerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public decimal TotalPurchases { get; set; }
    public decimal DebtAmount { get; set; }
    public Guid BranchId { get; set; }
}

internal sealed class CreateCustomerApiRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}
