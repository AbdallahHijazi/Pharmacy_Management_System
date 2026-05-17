using Pharmacy.Application.Common.Accounting;
using Pharmacy.Application.Common.Inventory;
using Pharmacy.Domain.Entities.Catalog;
using Pharmacy.Domain.Enums;
using Pharmacy.Domain.Exceptions;
using Xunit;

namespace Pharmacy.IntegrationTests;

public sealed class StockBatchManualAdjustmentTests
{
    [Fact]
    public void AdjustmentOut_changes_available_only_received_unchanged()
    {
        var batch = BatchWithBonus();
        var effectiveBefore = StockBatchEffectiveUnitCost.Calculate(batch);

        Assert.True(StockBatchManualAdjustment.TryGetQuantityDeltas(
            TransactionType.AdjustmentOut, 5, out var availDelta, out var recvDelta, out var bonusDelta));
        Assert.Equal(-5, availDelta);
        Assert.Equal(0, recvDelta);
        Assert.Equal(0, bonusDelta);

        StockBatchManualAdjustment.Apply(batch, availDelta, recvDelta, bonusDelta);

        Assert.Equal(60, batch.AvailableQuantity);
        Assert.Equal(65, batch.ReceivedQuantity);
        Assert.Equal(15, batch.BonusQuantity);
        Assert.Equal(effectiveBefore, StockBatchEffectiveUnitCost.Calculate(batch));
    }

    [Fact]
    public void AdjustmentIn_increases_received_and_available_preserving_invariant()
    {
        var batch = BatchWithBonus();
        var effectiveBefore = StockBatchEffectiveUnitCost.Calculate(batch);

        StockBatchManualAdjustment.Apply(batch, availableDelta: 10, receivedDelta: 10, bonusDelta: 10);

        Assert.Equal(75, batch.ReceivedQuantity);
        Assert.Equal(75, batch.AvailableQuantity);
        Assert.Equal(25, batch.BonusQuantity);
        Assert.NotEqual(effectiveBefore, StockBatchEffectiveUnitCost.Calculate(batch));
        Assert.Equal(100m * 50m / 75m, StockBatchEffectiveUnitCost.Calculate(batch));
    }

    [Fact]
    public void Apply_rejects_negative_available()
    {
        var batch = new StockBatch
        {
            PurchasePrice = 10m,
            ReceivedQuantity = 10,
            AvailableQuantity = 3,
            BonusQuantity = 0
        };

        var ex = Assert.Throws<BadRequestException>(() =>
            StockBatchManualAdjustment.Apply(batch, -5, 0, 0));

        Assert.Contains("أقل من صفر", ex.Message);
    }

    [Fact]
    public void Apply_rejects_available_exceeding_received()
    {
        var batch = new StockBatch
        {
            PurchasePrice = 10m,
            ReceivedQuantity = 10,
            AvailableQuantity = 10,
            BonusQuantity = 0
        };

        var ex = Assert.Throws<BadRequestException>(() =>
            StockBatchManualAdjustment.Apply(batch, 3, 0, 0));

        Assert.Contains("تتجاوز الكمية المستلمة", ex.Message);
    }

    [Fact]
    public void HasInvalidCostBasis_when_all_received_units_are_bonus()
    {
        var batch = new StockBatch
        {
            PurchasePrice = 100m,
            ReceivedQuantity = 10,
            AvailableQuantity = 5,
            BonusQuantity = 10
        };

        Assert.True(StockBatchEffectiveUnitCost.HasInvalidCostBasis(batch));
        Assert.Equal(0m, StockBatchEffectiveUnitCost.Calculate(batch));
    }

    [Fact]
    public void PurchaseIn_is_not_allowed_for_manual_adjustment_endpoint()
    {
        Assert.False(StockBatchManualAdjustment.IsAllowed(TransactionType.PurchaseIn));
        Assert.False(StockBatchManualAdjustment.IsAllowed(TransactionType.SaleOut));
        Assert.True(StockBatchManualAdjustment.IsAllowed(TransactionType.ExpiredWriteOff));
    }

    private static StockBatch BatchWithBonus() => new()
    {
        PurchasePrice = 100m,
        ReceivedQuantity = 65,
        AvailableQuantity = 65,
        BonusQuantity = 15
    };
}
