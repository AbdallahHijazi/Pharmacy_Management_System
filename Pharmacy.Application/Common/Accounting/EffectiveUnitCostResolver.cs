namespace Pharmacy.Application.Common.Accounting
{
    /// <summary>استنتاج تكلفة الوحدة الفعّالة لسطر بيع من اللقطات أو من حقول الدفعة المجمّعة.</summary>
    public static class EffectiveUnitCostResolver
    {
        public static decimal ResolveForSaleLine(
            decimal? unitEffectiveCostAtSale,
            decimal? batchNominalPurchasePriceAtSale,
            int? batchReceivedQuantityAtSale,
            int? batchBonusQuantityAtSale)
        {
            if (unitEffectiveCostAtSale.HasValue)
                return unitEffectiveCostAtSale.Value;

            var fromSnapshots = FromBatchSnapshots(
                batchNominalPurchasePriceAtSale,
                batchReceivedQuantityAtSale,
                batchBonusQuantityAtSale);

            return fromSnapshots ?? 0m;
        }

        public static decimal? FromBatchSnapshots(
            decimal? nominal,
            int? received,
            int? bonus)
        {
            if (!nominal.HasValue || !received.HasValue || !bonus.HasValue)
                return null;

            var recv = received.Value;
            if (recv <= 0)
                return nominal;

            var paidUnits = recv - bonus.Value;
            if (paidUnits <= 0)
                return 0m;

            return nominal.Value * paidUnits / recv;
        }
    }
}
