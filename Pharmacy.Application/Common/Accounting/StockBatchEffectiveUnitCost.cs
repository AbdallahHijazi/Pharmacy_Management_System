using Pharmacy.Domain.Entities.Catalog;

namespace Pharmacy.Application.Common.Accounting
{
    /// <summary>
    /// تكلفة الوحدة الفعلية للدفعة: يوزّع المبلغ المدفوع (سعر × كمية مدفوعة) على كل الوحدات الفيزيائية بما فيها البونص.
    /// </summary>
    public static class StockBatchEffectiveUnitCost
    {
        public static decimal Calculate(StockBatch batch)
        {
            if (batch.ReceivedQuantity <= 0)
                return batch.PurchasePrice;

            var paidUnits = batch.ReceivedQuantity - batch.BonusQuantity;
            if (paidUnits <= 0)
                return 0m;

            return (batch.PurchasePrice * paidUnits) / batch.ReceivedQuantity;
        }
    }
}
