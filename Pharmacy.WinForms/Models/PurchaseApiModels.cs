namespace Pharmacy.WinForms.Models;

internal sealed class PagedPurchaseInvoicesApiModel
{
    public List<PurchaseInvoiceApiModel> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

internal sealed class PurchaseInvoiceApiModel
{
    public Guid PurchaseInvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

internal sealed class PurchaseInvoiceDetailsApiModel
{
    public Guid PurchaseInvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

internal sealed class PurchaseInvoiceItemApiModel
{
    public Guid PurchaseInvoiceItemId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string BatchNumber { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public int Quantity { get; set; }
    public int BonusQuantity { get; set; }
    public decimal UnitPrice { get; set; }
}

internal sealed class SupplierListApiModel
{
    public Guid SupplierId { get; set; }
    public string Name { get; set; } = string.Empty;
}

internal sealed class PagedSuppliersApiModel
{
    public List<SupplierListApiModel> Items { get; set; } = new();
    public int TotalCount { get; set; }
}
