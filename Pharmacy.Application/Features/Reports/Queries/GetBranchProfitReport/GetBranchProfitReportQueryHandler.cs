using MediatR;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Common.Accounting;
using Pharmacy.Application.Common.Interfaces;
using Pharmacy.Application.DTOs.Reports;
using Pharmacy.Domain.Entities.Inventory;
using Pharmacy.Domain.Entities.Sales;
using Pharmacy.Domain.Enums;
using Pharmacy.Domain.Exceptions;

namespace Pharmacy.Application.Features.Reports.Queries.GetBranchProfitReport
{
    public sealed class GetBranchProfitReportQueryHandler : IRequestHandler<GetBranchProfitReportQuery, BranchProfitReportDto>
    {
        private readonly IRepository<SalesInvoice> _salesInvoiceRepository;
        private readonly IRepository<SalesInvoiceItem> _salesInvoiceItemRepository;
        private readonly IRepository<SalesReturn> _salesReturnRepository;
        private readonly IRepository<InventoryTransaction> _inventoryTransactionRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetBranchProfitReportQueryHandler(
            IRepository<SalesInvoice> salesInvoiceRepository,
            IRepository<SalesInvoiceItem> salesInvoiceItemRepository,
            IRepository<SalesReturn> salesReturnRepository,
            IRepository<InventoryTransaction> inventoryTransactionRepository,
            ICurrentUserService currentUserService)
        {
            _salesInvoiceRepository = salesInvoiceRepository;
            _salesInvoiceItemRepository = salesInvoiceItemRepository;
            _salesReturnRepository = salesReturnRepository;
            _inventoryTransactionRepository = inventoryTransactionRepository;
            _currentUserService = currentUserService;
        }

        public async Task<BranchProfitReportDto> Handle(GetBranchProfitReportQuery request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (_currentUserService.BranchId is null)
                throw new UnauthorizedException("لا يوجد فرع مرتبط بالمستخدم");

            if (request.FromDate > request.ToDate)
                throw new BadRequestException("تاريخ البداية يجب أن يكون قبل تاريخ النهاية");

            var branchId = _currentUserService.BranchId.Value;

            var invoicesInPeriod = await _salesInvoiceRepository
                .GetAllAsNoTracking()
                .Where(si =>
                    !si.IsDeleted &&
                    si.BranchId == branchId &&
                    si.CreatedAt >= request.FromDate &&
                    si.CreatedAt <= request.ToDate)
                .Select(si => new
                {
                    si.Id,
                    si.Subtotal,
                    si.DiscountAmount,
                    si.TaxAmount,
                    si.GrandTotal
                })
                .ToListAsync(cancellationToken);

            var invoiceIds = invoicesInPeriod.Select(i => i.Id).ToList();

            decimal salesCogs = 0;
            var missingCostLines = 0;

            if (invoiceIds.Count > 0)
            {
                var saleLines = await _salesInvoiceItemRepository
                    .GetAllAsNoTracking()
                    .Where(li => !li.IsDeleted && invoiceIds.Contains(li.SalesInvoiceId))
                    .Select(li => new
                    {
                        li.Id,
                        li.Quantity,
                        li.UnitEffectiveCostAtSale,
                        li.BatchNominalPurchasePriceAtSale,
                        li.BatchReceivedQuantityAtSale,
                        li.BatchBonusQuantityAtSale
                    })
                    .ToListAsync(cancellationToken);

                foreach (var li in saleLines)
                {
                    var unit = EffectiveUnitCostResolver.ResolveForSaleLine(
                        li.UnitEffectiveCostAtSale,
                        li.BatchNominalPurchasePriceAtSale,
                        li.BatchReceivedQuantityAtSale,
                        li.BatchBonusQuantityAtSale);

                    if (!li.UnitEffectiveCostAtSale.HasValue &&
                        !EffectiveUnitCostResolver.FromBatchSnapshots(
                            li.BatchNominalPurchasePriceAtSale,
                            li.BatchReceivedQuantityAtSale,
                            li.BatchBonusQuantityAtSale).HasValue)
                    {
                        missingCostLines++;
                    }

                    salesCogs += unit * li.Quantity;
                }
            }

            var grossSalesBeforeDiscount = invoicesInPeriod.Sum(i => i.Subtotal);
            var invoiceDiscountTotal = invoicesInPeriod.Sum(i => i.DiscountAmount);
            var taxOnSalesTotal = invoicesInPeriod.Sum(i => i.TaxAmount);
            var netSalesFromInvoices = invoicesInPeriod.Sum(i => i.GrandTotal);

            var returnsInPeriod = await _salesReturnRepository
                .GetAllAsNoTracking()
                .Include(sr => sr.SalesInvoice)
                .Include(sr => sr.Items)
                .Where(sr =>
                    !sr.IsDeleted &&
                    !sr.SalesInvoice.IsDeleted &&
                    sr.SalesInvoice.BranchId == branchId &&
                    sr.CreatedAt >= request.FromDate &&
                    sr.CreatedAt <= request.ToDate)
                .ToListAsync(cancellationToken);

            var salesReturnsRefund = returnsInPeriod.Sum(r => r.RefundAmount);

            var returnItemLineIds = returnsInPeriod
                .SelectMany(r => r.Items.Where(i => !i.IsDeleted && i.SalesInvoiceItemId.HasValue))
                .Select(i => i.SalesInvoiceItemId!.Value)
                .Distinct()
                .ToList();

            var returnCostByLineId = await LoadUnitCostsByLineIdsAsync(returnItemLineIds, cancellationToken);

            var fifoKeys = returnsInPeriod
                .SelectMany(r => r.Items
                    .Where(i => !i.IsDeleted && !i.SalesInvoiceItemId.HasValue)
                    .Select(i => (r.SalesInvoiceId, i.StockBatchId)))
                .Distinct()
                .ToList();

            var fifoBuckets = new Dictionary<(Guid InvoiceId, Guid BatchId), List<FifoCostSegment>>();
            foreach (var (invId, batchId) in fifoKeys)
            {
                var lines = await _salesInvoiceItemRepository
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

            decimal salesReturnCogsRecovery = 0;

            foreach (var ret in returnsInPeriod.OrderBy(r => r.CreatedAt).ThenBy(r => r.Id))
            {
                foreach (var ri in ret.Items.Where(i => !i.IsDeleted).OrderBy(i => i.Id))
                {
                    if (ri.SalesInvoiceItemId is Guid sid)
                    {
                        if (returnCostByLineId.TryGetValue(sid, out var uc))
                            salesReturnCogsRecovery += uc * ri.Quantity;
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
                        salesReturnCogsRecovery += take * seg.UnitCost;
                        seg.RemainingQty -= take;
                        need -= take;
                    }
                }
            }

            var prMovements = await _inventoryTransactionRepository
                .GetAllAsNoTracking()
                .Where(t =>
                    !t.IsDeleted &&
                    t.BranchId == branchId &&
                    t.CreatedAt >= request.FromDate &&
                    t.CreatedAt <= request.ToDate &&
                    t.Type == TransactionType.PurchaseReturnOut &&
                    t.ReferenceType == ReferenceType.PurchaseReturn)
                .Include(t => t.StockBatch)
                .ToListAsync(cancellationToken);

            decimal purchaseReturnCogsRecovery = 0;
            foreach (var t in prMovements)
            {
                var eff = StockBatchEffectiveUnitCost.Calculate(t.StockBatch);
                purchaseReturnCogsRecovery += eff * t.Quantity;
            }

            var snapshot = new BranchProfitRawSnapshot(
                branchId,
                request.FromDate,
                request.ToDate,
                invoicesInPeriod.Count,
                grossSalesBeforeDiscount,
                invoiceDiscountTotal,
                taxOnSalesTotal,
                netSalesFromInvoices,
                salesReturnsRefund,
                returnsInPeriod.Count,
                salesCogs,
                salesReturnCogsRecovery,
                missingCostLines,
                purchaseReturnCogsRecovery,
                prMovements.Count);

            return BranchProfitReportCalculator.Calculate(snapshot);
        }

        private async Task<Dictionary<Guid, decimal>> LoadUnitCostsByLineIdsAsync(
            List<Guid> lineIds,
            CancellationToken cancellationToken)
        {
            if (lineIds.Count == 0)
                return new Dictionary<Guid, decimal>();

            var rows = await _salesInvoiceItemRepository
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
