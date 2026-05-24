namespace Pharmacy.WinForms.Models;

internal sealed class PagedSupplierItemsApiModel
{
    public List<SupplierListItemApiModel> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

internal sealed class SupplierListItemApiModel
{
    public Guid SupplierId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public decimal TotalPurchases { get; set; }
    public decimal PayableAmount { get; set; }
    public Guid BranchId { get; set; }
}

internal sealed class SupplierDetailsApiModel
{
    public Guid SupplierId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public decimal TotalPurchases { get; set; }
    public decimal PayableAmount { get; set; }
    public Guid BranchId { get; set; }
}

internal sealed class CreateSupplierApiRequest
{
    public string Name { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}

internal sealed class UpdateSupplierApiRequest
{
    public string Name { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}
