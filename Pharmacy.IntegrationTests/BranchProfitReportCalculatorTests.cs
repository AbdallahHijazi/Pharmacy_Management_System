using Pharmacy.Application.Common.Accounting;
using Xunit;

namespace Pharmacy.IntegrationTests;

public sealed class BranchProfitReportCalculatorTests
{
    [Fact]
    public void Net_profit_equals_net_sales_after_returns_minus_net_cogs()
    {
        var branchId = Guid.NewGuid();
        var from = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var to = new DateTime(2026, 1, 31, 0, 0, 0, DateTimeKind.Utc);

        var snapshot = new BranchProfitRawSnapshot(
            branchId,
            from,
            to,
            SalesInvoiceCount: 2,
            GrossSalesBeforeDiscount: 200m,
            InvoiceDiscountTotal: 20m,
            TaxOnSalesTotal: 0m,
            NetSalesFromInvoices: 180m,
            SalesReturnsRefundTotal: 30m,
            SalesReturnCount: 1,
            SalesCogsTotal: 80m,
            SalesReturnCogsRecoveryTotal: 12m,
            SaleLinesMissingUnitCost: 0,
            PurchaseReturnCogsRecoveryTotal: 8m,
            PurchaseReturnMovementCount: 1);

        var r = BranchProfitReportCalculator.Calculate(snapshot);

        Assert.Equal(150m, r.NetSalesAfterReturns);
        Assert.Equal(60m, r.NetCogs);
        Assert.Equal(90m, r.GrossProfit);
        Assert.Equal(90m, r.NetProfit);
        Assert.Equal(60m, r.GrossProfitMarginPercent);
    }
}
