namespace Pharmacy.Application.Common.Accounting
{
    /// <summary>مدخلات مجمّعة من قاعدة البيانات قبل تطبيق معادلة الربح.</summary>
    public sealed record BranchProfitRawSnapshot(
        Guid BranchId,
        DateTime FromUtc,
        DateTime ToUtc,
        int SalesInvoiceCount,
        decimal GrossSalesBeforeDiscount,
        decimal InvoiceDiscountTotal,
        decimal TaxOnSalesTotal,
        decimal NetSalesFromInvoices,
        decimal SalesReturnsRefundTotal,
        int SalesReturnCount,
        decimal SalesCogsTotal,
        decimal SalesReturnCogsRecoveryTotal,
        int SaleLinesMissingUnitCost,
        decimal PurchaseReturnCogsRecoveryTotal,
        int PurchaseReturnMovementCount);
}
