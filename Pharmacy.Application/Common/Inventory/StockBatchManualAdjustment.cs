using Pharmacy.Domain.Entities.Catalog;
using Pharmacy.Domain.Enums;
using Pharmacy.Domain.Exceptions;

namespace Pharmacy.Application.Common.Inventory
{
    /// <summary>
    /// سياسة التعديل اليدوي للدفعة (تصحيح، تالف، منتهي، فاقد/زيادة):
    /// <list type="bullet">
    /// <item><description>الخروج (AdjustmentOut / ExpiredWriteOff): يخصم <see cref="StockBatch.AvailableQuantity"/> فقط — لا يغيّر <see cref="StockBatch.ReceivedQuantity"/> ولا <see cref="StockBatch.BonusQuantity"/>، فيبقى EffectiveUnitCost ثابتًا.</description></item>
    /// <item><description>الدخول (AdjustmentIn): يزيد المتاح والمستلم والبونص معًا — الوحدات المكتسبة تُسجَّل كبونص (تكلفة شراء إضافية صفر) فيُخفَّف EffectiveUnitCost دون تغيير الوحدات المدفوعة.</description></item>
    /// </list>
    /// لإضافة دفعة جديدة أو شراء استخدم مسارات PurchaseIn / ManualBatchIn وليس adjust-stock.
    /// </summary>
    public static class StockBatchManualAdjustment
    {
        public static bool IsAllowed(TransactionType type) =>
            type is TransactionType.AdjustmentIn
                or TransactionType.AdjustmentOut
                or TransactionType.ExpiredWriteOff;

        public static bool TryGetQuantityDeltas(
            TransactionType type,
            int quantity,
            out int availableDelta,
            out int receivedDelta,
            out int bonusDelta)
        {
            availableDelta = 0;
            receivedDelta = 0;
            bonusDelta = 0;

            switch (type)
            {
                case TransactionType.AdjustmentIn:
                    availableDelta = quantity;
                    receivedDelta = quantity;
                    bonusDelta = quantity;
                    return true;
                case TransactionType.AdjustmentOut:
                case TransactionType.ExpiredWriteOff:
                    availableDelta = -quantity;
                    receivedDelta = 0;
                    bonusDelta = 0;
                    return true;
                default:
                    return false;
            }
        }

        public static void Apply(StockBatch batch, int availableDelta, int receivedDelta, int bonusDelta)
        {
            batch.AvailableQuantity += availableDelta;
            batch.ReceivedQuantity += receivedDelta;
            batch.BonusQuantity += bonusDelta;
            ValidateInvariants(batch);
        }

        /// <summary>يتحقق من اتساق كميات الدفعة بعد أي تعديل يدوي.</summary>
        public static void ValidateInvariants(StockBatch batch)
        {
            if (batch.ReceivedQuantity < 0)
                throw new BadRequestException("الكمية المستلمة للدفعة لا يمكن أن تكون سالبة.");

            if (batch.BonusQuantity < 0)
                throw new BadRequestException("كمية البونص لا يمكن أن تكون سالبة.");

            if (batch.BonusQuantity > batch.ReceivedQuantity)
                throw new BadRequestException("كمية البونص لا يمكن أن تتجاوز الكمية المستلمة.");

            if (batch.AvailableQuantity < 0)
                throw new BadRequestException("لا يمكن أن تصبح الكمية المتاحة أقل من صفر.");

            if (batch.AvailableQuantity > batch.ReceivedQuantity)
                throw new BadRequestException(
                    "الكمية المتاحة لا يمكن أن تتجاوز الكمية المستلمة. لزيادة المخزون استخدم AdjustmentIn أو أضف دفعة عبر الشراء.");
        }
    }
}
