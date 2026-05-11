using System.Linq.Expressions;
using Pharmacy.Domain.Entities.Catalog;

namespace Pharmacy.Application.Common.Inventory
{
    /// <summary>
    /// قواعد موحّدة لحساب كميات المنتج من دفعات المخزون النشطة (غير المحذوفة + نفس الفرع).
    /// </summary>
    public static class ProductAvailableQuantity
    {
        /// <summary>دفعات تُحسب ضمن مخزون الفرع للمنتج.</summary>
        public static Expression<Func<StockBatch, bool>> ActiveBatchesInBranch(Guid branchId) =>
            b => b.BranchId == branchId && !b.IsDeleted;

        /// <summary>
        /// تجميع موحّد: مجموع المتاح، القابل للبيع، والمنتهي مع رصيد — لنفس تعريف الدفعات النشطة.
        /// </summary>
        public static IQueryable<ProductStockAggregateDto> GetStockAggregatesByProductIds(
            IQueryable<StockBatch> stockBatches,
            Guid branchId,
            DateTime asOfUtc,
            IReadOnlyList<Guid> productIds) =>
            stockBatches
                .Where(ActiveBatchesInBranch(branchId))
                .Where(b => productIds.Contains(b.ProductId))
                .GroupBy(b => b.ProductId)
                .Select(g => new ProductStockAggregateDto
                {
                    ProductId = g.Key,
                    TotalAvailableQuantity = g.Sum(x => x.AvailableQuantity),
                    SellableQuantity = g.Sum(x =>
                        x.AvailableQuantity > 0 && x.ExpiryDate > asOfUtc ? x.AvailableQuantity : 0),
                    ExpiredQuantity = g.Sum(x =>
                        x.AvailableQuantity > 0 && x.ExpiryDate <= asOfUtc ? x.AvailableQuantity : 0)
                });
    }

    /// <summary>نتيجة تجميع كميات الدفعات لمنتج واحد في فرع.</summary>
    public sealed class ProductStockAggregateDto
    {
        public Guid ProductId { get; set; }
        public int TotalAvailableQuantity { get; set; }
        public int SellableQuantity { get; set; }
        public int ExpiredQuantity { get; set; }
    }
}
