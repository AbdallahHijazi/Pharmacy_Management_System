using Pharmacy.Application.Common.Accounting;
using Pharmacy.Domain.Entities.Catalog;
using Xunit;

namespace Pharmacy.IntegrationTests;

public sealed class StockBatchEffectiveUnitCostTests
{
    [Fact]
    public void Calculate_with_bonus_dilutes_unit_cost()
    {
        var batch = new StockBatch
        {
            PurchasePrice = 100m,
            ReceivedQuantity = 65,
            BonusQuantity = 15
        };

        var effective = StockBatchEffectiveUnitCost.Calculate(batch);

        Assert.Equal(5000m / 65m, effective);
    }

    [Fact]
    public void Calculate_without_bonus_equals_purchase_price()
    {
        var batch = new StockBatch
        {
            PurchasePrice = 3m,
            ReceivedQuantity = 10,
            BonusQuantity = 0
        };

        Assert.Equal(3m, StockBatchEffectiveUnitCost.Calculate(batch));
    }

    [Fact]
    public void Purchase_return_refund_formula_matches_effective_cost_times_quantity()
    {
        var batch = new StockBatch
        {
            PurchasePrice = 100m,
            ReceivedQuantity = 65,
            BonusQuantity = 15
        };

        const int returnQty = 10;
        var effective = StockBatchEffectiveUnitCost.Calculate(batch);
        var refund = returnQty * effective;

        Assert.Equal(returnQty * 50m * 100m / 65m, refund);
        Assert.NotEqual(returnQty * batch.PurchasePrice, refund);
    }
}
