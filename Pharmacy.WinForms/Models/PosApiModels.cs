namespace Pharmacy.WinForms.Models;

internal sealed class PagedProductsApiModel
{
    public List<PosProductApiModel> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

internal sealed class PosProductApiModel
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string TradeName { get; set; } = string.Empty;
    public string CommercialName { get; set; } = string.Empty;
    public string ArabicName { get; set; } = string.Empty;
    public string ScientificName { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal SellingPrice { get; set; }
    public int PricingType { get; set; }
    public int TotalAvailableQuantity { get; set; }
    public int SellableQuantity { get; set; }
    public int ExpiredQuantity { get; set; }
}

internal sealed class CreateSalesInvoiceApiRequest
{
    public Guid? CustomerId { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal PaidAmount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public List<CreateSalesInvoiceItemApiRequest> Items { get; set; } = new();
}

internal sealed class CreateSalesInvoiceItemApiRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

internal sealed class SalesInvoiceCreatedApiModel
{
    public Guid SalesInvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal GrandTotal { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public string Status { get; set; } = string.Empty;
}
