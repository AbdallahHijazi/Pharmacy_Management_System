namespace Pharmacy.WinForms.Models;

internal sealed class PagedInventoryProductsApiModel
{
    public List<InventoryProductApiModel> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

internal sealed class InventoryProductApiModel
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ScientificName { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal SellingPrice { get; set; }
    public int PricingType { get; set; }
    public decimal? PurchasePrice { get; set; }
    public Guid? DefaultSupplierId { get; set; }
    public int TotalAvailableQuantity { get; set; }
    public int SellableQuantity { get; set; }
    public int ExpiredQuantity { get; set; }
}

internal sealed class InventoryProductDetailsApiModel
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ScientificName { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal SellingPrice { get; set; }
    public int PricingType { get; set; }
    public decimal? PurchasePrice { get; set; }
    public int TotalAvailableQuantity { get; set; }
    public int SellableQuantity { get; set; }
    public int ExpiredQuantity { get; set; }
    public Guid? DefaultSupplierId { get; set; }
}

internal sealed class InventoryReportApiModel
{
    public int TotalProductsInStock { get; set; }
    public int TotalAvailableQuantity { get; set; }
    public int LowStockBatchesCount { get; set; }
    public int ExpiringSoonBatchesCount { get; set; }
    public int ExpiredBatchesCount { get; set; }
}

internal sealed class InventoryStockBatchApiModel
{
    public Guid StockBatchId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string LotNumber { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public decimal PurchasePrice { get; set; }
    public int ReceivedQuantity { get; set; }
    public int BonusQuantity { get; set; }
    public int AvailableQuantity { get; set; }
    public Guid SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
}

internal sealed class InventoryTransactionApiModel
{
    public Guid InventoryTransactionId { get; set; }
    public Guid StockBatchId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string BatchNumber { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string ReferenceType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

internal sealed class InventoryLowStockProductApiModel
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int TotalAvailableQuantity { get; set; }
    public int BatchesCount { get; set; }
}
