using Pharmacy.Application.Common.Accounting;
using Xunit;

namespace Pharmacy.IntegrationTests;

public sealed class EffectiveUnitCostResolverTests
{
    [Fact]
    public void ResolveForSaleLine_prefers_stored_unit_effective_cost()
    {
        var unit = EffectiveUnitCostResolver.ResolveForSaleLine(
            unitEffectiveCostAtSale: 76.92m,
            batchNominalPurchasePriceAtSale: 100m,
            batchReceivedQuantityAtSale: 65,
            batchBonusQuantityAtSale: 15);

        Assert.Equal(76.92m, unit);
    }

    [Fact]
    public void FromBatchSnapshots_computes_bonus_diluted_cost()
    {
        var unit = EffectiveUnitCostResolver.FromBatchSnapshots(
            nominal: 100m,
            received: 65,
            bonus: 15);

        Assert.Equal(100m * 50m / 65m, unit);
    }

    [Fact]
    public void ResolveForSaleLine_falls_back_to_batch_snapshots_when_unit_cost_missing()
    {
        var unit = EffectiveUnitCostResolver.ResolveForSaleLine(
            unitEffectiveCostAtSale: null,
            batchNominalPurchasePriceAtSale: 5m,
            batchReceivedQuantityAtSale: 10,
            batchBonusQuantityAtSale: 2);

        Assert.Equal(5m * 8m / 10m, unit);
    }

    [Fact]
    public void ResolveForSaleLine_returns_zero_when_no_cost_data()
    {
        var unit = EffectiveUnitCostResolver.ResolveForSaleLine(
            null, null, null, null);

        Assert.Equal(0m, unit);
    }
}
