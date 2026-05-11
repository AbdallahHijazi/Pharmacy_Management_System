using Pharmacy.Application.DTOs.Reports;

namespace Pharmacy.Application.Common.Accounting
{
    /// <summary>
    /// Net Sales − Net COGS (مع عكس تكلفة مرتجعات البيع ومرتجعات الشراء على أساس تكلفة الدفعة الفعّالة).
    /// لا يشمل مصاريف تشغيل — NetProfit يساوي GrossProfit حتى إضافة نموذج مصاريف.
    /// </summary>
    public static class BranchProfitReportCalculator
    {
        public static BranchProfitReportDto Calculate(BranchProfitRawSnapshot s)
        {
            var netSalesAfterReturns = s.NetSalesFromInvoices - s.SalesReturnsRefundTotal;
            var netCogs = s.SalesCogsTotal - s.SalesReturnCogsRecoveryTotal - s.PurchaseReturnCogsRecoveryTotal;
            var grossProfit = netSalesAfterReturns - netCogs;
            var netProfit = grossProfit;
            decimal? marginPercent = netSalesAfterReturns > 0
                ? Math.Round(grossProfit / netSalesAfterReturns * 100m, 4, MidpointRounding.AwayFromZero)
                : null;

            return new BranchProfitReportDto
            {
                BranchId = s.BranchId,
                FromUtc = s.FromUtc,
                ToUtc = s.ToUtc,
                SalesInvoiceCount = s.SalesInvoiceCount,
                GrossSalesBeforeDiscount = s.GrossSalesBeforeDiscount,
                InvoiceDiscountTotal = s.InvoiceDiscountTotal,
                TaxOnSalesTotal = s.TaxOnSalesTotal,
                NetSalesFromInvoices = s.NetSalesFromInvoices,
                SalesReturnCount = s.SalesReturnCount,
                SalesReturnsRefundTotal = s.SalesReturnsRefundTotal,
                NetSalesAfterReturns = netSalesAfterReturns,
                SalesCogsTotal = s.SalesCogsTotal,
                SalesReturnCogsRecoveryTotal = s.SalesReturnCogsRecoveryTotal,
                PurchaseReturnCogsRecoveryTotal = s.PurchaseReturnCogsRecoveryTotal,
                PurchaseReturnMovementCount = s.PurchaseReturnMovementCount,
                NetCogs = netCogs,
                GrossProfit = grossProfit,
                NetProfit = netProfit,
                GrossProfitMarginPercent = marginPercent,
                SaleLinesMissingUnitCostCount = s.SaleLinesMissingUnitCost
            };
        }
    }
}
