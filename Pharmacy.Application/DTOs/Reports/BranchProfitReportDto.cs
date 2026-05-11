namespace Pharmacy.Application.DTOs.Reports
{
    /// <summary>تقرير ربحية الفرع لفترة — قابل للتدقيق من فواتير البيع والدفعات والمرتجعات.</summary>
    public sealed class BranchProfitReportDto
    {
        public Guid BranchId { get; set; }
        public DateTime FromUtc { get; set; }
        public DateTime ToUtc { get; set; }

        public int SalesInvoiceCount { get; set; }
        public decimal GrossSalesBeforeDiscount { get; set; }
        public decimal InvoiceDiscountTotal { get; set; }
        public decimal TaxOnSalesTotal { get; set; }
        /// <summary>مجموع صافي فواتير البيع في الفترة (عادة مجموع GrandTotal).</summary>
        public decimal NetSalesFromInvoices { get; set; }

        public int SalesReturnCount { get; set; }
        public decimal SalesReturnsRefundTotal { get; set; }
        /// <summary>صافي المبيعات بعد خصم مبالغ مرتجعات البيع المعترف بها في الفترة.</summary>
        public decimal NetSalesAfterReturns { get; set; }

        public decimal SalesCogsTotal { get; set; }
        /// <summary>عكس تكلفة البضاعة لمرتجعات البيع (تقليل COGS).</summary>
        public decimal SalesReturnCogsRecoveryTotal { get; set; }
        /// <summary>تكلفة وحدات خرجت بمرتجع شراء (تقليل COGS/تكلفة مشتريات).</summary>
        public decimal PurchaseReturnCogsRecoveryTotal { get; set; }
        public int PurchaseReturnMovementCount { get; set; }

        public decimal NetCogs { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal NetProfit { get; set; }
        public decimal? GrossProfitMarginPercent { get; set; }

        /// <summary>أسطر بيع بدون لقطة تكلفة فعّالة — تُحسب التكلفة 0 حيث لا يمكن الاستنتاج.</summary>
        public int SaleLinesMissingUnitCostCount { get; set; }
    }
}
