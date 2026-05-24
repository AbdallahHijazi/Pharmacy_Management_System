namespace Pharmacy.WinForms.Models;

internal sealed class SalesReportApiModel
{
    public int TotalInvoices { get; set; }
    public decimal TotalSales { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal TotalRemaining { get; set; }
}

internal sealed class MonthlyFinancialReportApiModel
{
    public int Year { get; set; }
    public int Month { get; set; }
    public DateTime PeriodStartUtc { get; set; }
    public DateTime PeriodEndUtc { get; set; }
    public BranchProfitReportApiModel? Profit { get; set; }
    public decimal NetSalesAfterReturns { get; set; }
    public decimal NetCostOfGoodsSold { get; set; }
}

internal sealed class DailyFinancialReportFullApiModel
{
    public DateTime ReportDateUtc { get; set; }
    public BranchProfitReportApiModel? Profit { get; set; }
    public decimal NetSalesAfterReturns { get; set; }
    public decimal NetCostOfGoodsSold { get; set; }
}

internal sealed class BranchProfitReportApiModel
{
    public int SalesInvoiceCount { get; set; }
    public decimal NetSalesAfterReturns { get; set; }
    public decimal NetCogs { get; set; }
    public decimal GrossProfit { get; set; }
    public decimal NetProfit { get; set; }
    public decimal? GrossProfitMarginPercent { get; set; }
}

internal sealed class ProductProfitRankingReportApiModel
{
    public DateTime FromUtc { get; set; }
    public DateTime ToUtc { get; set; }
    public string Rank { get; set; } = string.Empty;
    public int Take { get; set; }
    public List<ProductProfitRowApiModel> Rows { get; set; } = new();
}

internal sealed class ProductProfitRowApiModel
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int SoldQuantity { get; set; }
    public int ReturnedQuantity { get; set; }
    public decimal NetSales { get; set; }
    public decimal NetCostOfGoodsSold { get; set; }
    public decimal GrossProfit { get; set; }
}

internal sealed class TopSellingProductApiModel
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int TotalSoldQuantity { get; set; }
    public decimal TotalSalesAmount { get; set; }
}
