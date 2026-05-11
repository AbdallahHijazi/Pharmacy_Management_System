namespace Pharmacy.Application.DTOs.Reports
{
    public sealed class ProductProfitRankingReportDto
    {
        public DateTime FromUtc { get; set; }
        public DateTime ToUtc { get; set; }
        public string Rank { get; set; } = string.Empty;
        public int Take { get; set; }
        public IReadOnlyList<ProductProfitRowDto> Rows { get; set; } = Array.Empty<ProductProfitRowDto>();
    }

    public sealed class ProductProfitRowDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int SoldQuantity { get; set; }
        public int ReturnedQuantity { get; set; }
        /// <summary>صافي إيراد السطر بعد توزيع خصم الفاتورة، ناقص مبالغ مرتجعات البيع المنسوبة للمنتج.</summary>
        public decimal NetSales { get; set; }
        /// <summary>تكلفة البضاعة المباعة للمنتج ناقص استرداد تكلفة المرتجعات المنسوبة.</summary>
        public decimal NetCostOfGoodsSold { get; set; }
        public decimal GrossProfit { get; set; }
    }
}
