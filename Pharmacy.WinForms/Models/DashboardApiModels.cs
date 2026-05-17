namespace Pharmacy.WinForms.Models;

internal sealed class DashboardStatsApiModel
{
    public int TotalProducts { get; set; }
    public int TodayInvoicesCount { get; set; }
    public decimal TodaySalesTotal { get; set; }
    public int LowStockProductsCount { get; set; }
    public int ExpiringSoonBatchesCount { get; set; }
}

internal sealed class LatestSalesInvoiceApiModel
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal GrandTotal { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

internal sealed class LowStockProductApiModel
{
    public string ProductName { get; set; } = string.Empty;
    public int TotalAvailableQuantity { get; set; }
}

internal sealed class ExpiringSoonBatchApiModel
{
    public string ProductName { get; set; } = string.Empty;
    public string BatchNumber { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public int AvailableQuantity { get; set; }
}

internal sealed class DailyFinancialReportApiModel
{
    public BranchProfitApiModel? Profit { get; set; }
}

internal sealed class BranchProfitApiModel
{
    public decimal NetProfit { get; set; }
}
