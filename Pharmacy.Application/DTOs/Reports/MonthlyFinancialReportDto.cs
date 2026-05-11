namespace Pharmacy.Application.DTOs.Reports
{
    /// <summary>ملخص مالي لشهر تقويمي كامل (UTC) للفرع الحالي.</summary>
    public sealed class MonthlyFinancialReportDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public DateTime PeriodStartUtc { get; set; }
        public DateTime PeriodEndUtc { get; set; }
        public BranchProfitReportDto Profit { get; set; } = null!;

        public decimal NetSalesAfterReturns => Profit.NetSalesAfterReturns;
        public decimal NetCostOfGoodsSold => Profit.NetCogs;
    }
}
