using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Common.Interfaces;
using Pharmacy.Domain.Entities.Sales;

namespace Pharmacy.Application.Common.Accounting
{
    /// <summary>حساب استرداد تكلفة البضاعة لمرتجعات البيع (نفس منطق تقرير الربح) مع تفصيل لكل بند مرتجع.</summary>
    public static class SalesReturnCogsRecoveryEvaluator
    {
        public static async Task<(decimal TotalRecovery, Dictionary<Guid, decimal> RecoveryBySalesReturnItemId)> EvaluateAsync(
            IReadOnlyList<SalesReturn> returnsInPeriod,
            IRepository<SalesInvoiceItem> salesInvoiceItemRepository,
            CancellationToken cancellationToken)
        {
            var recoveryByItem = new Dictionary<Guid, decimal>();
            decimal total = 0;

            var returnItemLineIds = returnsInPeriod
                .SelectMany(r => r.Items.Where(i => !i.IsDeleted && i.SalesInvoiceItemId.HasValue))
                .Select(i => i.SalesInvoiceItemId!.Value)
                .Distinct()
                .ToList();

            var returnCostByLineId = await LoadUnitCostsByLineIdsAsync(
                salesInvoiceItemRepository,
                returnItemLineIds,
                cancellationToken);

            var fifoKeys = returnsInPeriod
                .SelectMany(r => r.Items
                    .Where(i => !i.IsDeleted && !i.SalesInvoiceItemId.HasValue)
                    .Select(i => (r.SalesInvoiceId, i.StockBatchId)))
                .Distinct()
                .ToList();

            var fifoBuckets = new Dictionary<(Guid InvoiceId, Guid BatchId), List<FifoCostSegment>>();
            foreach (var (invId, batchId) in fifoKeys)
            {
                var lines = await salesInvoiceItemRepository
                    .GetAllAsNoTracking()
                    .Where(li =>
                        !li.IsDeleted &&
                        li.SalesInvoiceId == invId &&
                        li.StockBatchId == batchId)
                    .OrderBy(li => li.Id)
                    .Select(li => new
                    {
                        li.Quantity,
                        li.UnitEffectiveCostAtSale,
                        li.BatchNominalPurchasePriceAtSale,
                        li.BatchReceivedQuantityAtSale,
                        li.BatchBonusQuantityAtSale
                    })
                    .ToListAsync(cancellationToken);

                var segs = new List<FifoCostSegment>();
                foreach (var li in lines)
                {
                    var u = EffectiveUnitCostResolver.ResolveForSaleLine(
                        li.UnitEffectiveCostAtSale,
                        li.BatchNominalPurchasePriceAtSale,
                        li.BatchReceivedQuantityAtSale,
                        li.BatchBonusQuantityAtSale);
                    segs.Add(new FifoCostSegment { RemainingQty = li.Quantity, UnitCost = u });
                }

                fifoBuckets[(invId, batchId)] = segs;
            }

            foreach (var ret in returnsInPeriod.OrderBy(r => r.CreatedAt).ThenBy(r => r.Id))
            {
                foreach (var ri in ret.Items.Where(i => !i.IsDeleted).OrderBy(i => i.Id))
                {
                    if (ri.SalesInvoiceItemId is Guid sid)
                    {
                        if (returnCostByLineId.TryGetValue(sid, out var uc))
                        {
                            var add = uc * ri.Quantity;
                            total += add;
                            recoveryByItem[ri.Id] = add;
                        }

                        continue;
                    }

                    if (!fifoBuckets.TryGetValue((ret.SalesInvoiceId, ri.StockBatchId), out var segments))
                        continue;

                    var need = ri.Quantity;
                    while (need > 0)
                    {
                        var seg = segments.FirstOrDefault(s => s.RemainingQty > 0);
                        if (seg is null)
                            break;

                        var take = Math.Min(seg.RemainingQty, need);
                        var add = take * seg.UnitCost;
                        total += add;
                        recoveryByItem[ri.Id] = recoveryByItem.GetValueOrDefault(ri.Id) + add;
                        seg.RemainingQty -= take;
                        need -= take;
                    }
                }
            }

            return (total, recoveryByItem);
        }

        private static async Task<Dictionary<Guid, decimal>> LoadUnitCostsByLineIdsAsync(
            IRepository<SalesInvoiceItem> salesInvoiceItemRepository,
            List<Guid> lineIds,
            CancellationToken cancellationToken)
        {
            if (lineIds.Count == 0)
                return new Dictionary<Guid, decimal>();

            var rows = await salesInvoiceItemRepository
                .GetAllAsNoTracking()
                .Where(li => lineIds.Contains(li.Id))
                .Select(li => new
                {
                    li.Id,
                    li.UnitEffectiveCostAtSale,
                    li.BatchNominalPurchasePriceAtSale,
                    li.BatchReceivedQuantityAtSale,
                    li.BatchBonusQuantityAtSale
                })
                .ToListAsync(cancellationToken);

            return rows.ToDictionary(
                r => r.Id,
                r => EffectiveUnitCostResolver.ResolveForSaleLine(
                    r.UnitEffectiveCostAtSale,
                    r.BatchNominalPurchasePriceAtSale,
                    r.BatchReceivedQuantityAtSale,
                    r.BatchBonusQuantityAtSale));
        }

        private sealed class FifoCostSegment
        {
            public int RemainingQty { get; set; }
            public decimal UnitCost { get; set; }
        }
    }
}
