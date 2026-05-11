namespace Pharmacy.Application.DTOs.Reports
{
    /// <summary>ملخص مالي ليوم تقويمي واحد (UTC) للفرع الحالي.</summary>
    public sealed class DailyFinancialReportDto
    {
        public DateTime ReportDateUtc { get; set; }
        public BranchProfitReportDto Profit { get; set; } = null!;

        public decimal NetSalesAfterReturns => Profit.NetSalesAfterReturns;
        public decimal NetCostOfGoodsSold => Profit.NetCogs;
    }
}
