namespace Pharmacy.WinForms.Models;

public sealed class DashboardSummary
{
    public int TotalProducts { get; init; }
    public decimal TodaySales { get; init; }
    public decimal TodayProfit { get; init; }
    public int LowStockProductsCount { get; init; }
    public int ExpiringSoonBatchesCount { get; init; }
    public int TodayInvoicesCount { get; init; }

    public IReadOnlyList<DashboardSaleRow> LatestSales { get; init; } = Array.Empty<DashboardSaleRow>();
    public IReadOnlyList<DashboardStockAlert> StockAlerts { get; init; } = Array.Empty<DashboardStockAlert>();
}

public sealed class DashboardSaleRow
{
    public string InvoiceNumber { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
    public decimal GrandTotal { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

public sealed class DashboardStockAlert
{
    public string Title { get; init; } = string.Empty;
    public string Detail { get; init; } = string.Empty;
    public bool IsExpiryAlert { get; init; }
    public string AlertKind { get; init; } = string.Empty;
    public string? BatchNumber { get; init; }
}

public sealed class DashboardLoadResult
{
    public DashboardSummary Summary { get; init; } = new();
    public bool IsMockData { get; init; }
    public string? ErrorMessage { get; init; }
    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
}
