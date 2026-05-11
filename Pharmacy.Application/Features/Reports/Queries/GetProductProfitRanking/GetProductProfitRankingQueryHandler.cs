using MediatR;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Common.Accounting;
using Pharmacy.Application.Common.Interfaces;
using Pharmacy.Application.DTOs.Reports;
using Pharmacy.Domain.Entities.Sales;
using Pharmacy.Domain.Exceptions;

namespace Pharmacy.Application.Features.Reports.Queries.GetProductProfitRanking
{
    public sealed class GetProductProfitRankingQueryHandler : IRequestHandler<GetProductProfitRankingQuery, ProductProfitRankingReportDto>
    {
        private readonly IRepository<SalesInvoice> _salesInvoiceRepository;
        private readonly IRepository<SalesInvoiceItem> _salesInvoiceItemRepository;
        private readonly IRepository<SalesReturn> _salesReturnRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetProductProfitRankingQueryHandler(
            IRepository<SalesInvoice> salesInvoiceRepository,
            IRepository<SalesInvoiceItem> salesInvoiceItemRepository,
            IRepository<SalesReturn> salesReturnRepository,
            ICurrentUserService currentUserService)
        {
            _salesInvoiceRepository = salesInvoiceRepository;
            _salesInvoiceItemRepository = salesInvoiceItemRepository;
            _salesReturnRepository = salesReturnRepository;
            _currentUserService = currentUserService;
        }

        public async Task<ProductProfitRankingReportDto> Handle(GetProductProfitRankingQuery request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (_currentUserService.BranchId is null)
                throw new UnauthorizedException("لا يوجد فرع مرتبط بالمستخدم");

            if (request.FromDate > request.ToDate)
                throw new BadRequestException("تاريخ البداية يجب أن يكون قبل تاريخ النهاية");

            var take = request.Take <= 0 ? 10 : Math.Min(request.Take, 50);
            var rank = (request.Rank ?? "BestProfit").Trim();
            var worst = rank.Equals("WorstProfit", StringComparison.OrdinalIgnoreCase);

            var branchId = _currentUserService.BranchId.Value;

            var invoicesInPeriod = await _salesInvoiceRepository
                .GetAllAsNoTracking()
                .Where(si =>
                    !si.IsDeleted &&
                    si.BranchId == branchId &&
                    si.CreatedAt >= request.FromDate &&
                    si.CreatedAt <= request.ToDate)
                .Select(si => si.Id)
                .ToListAsync(cancellationToken);

            var invoiceIds = invoicesInPeriod.ToHashSet();
            var agg = new Dictionary<Guid, ProductAgg>();

            if (invoiceIds.Count > 0)
            {
                var saleLines = await _salesInvoiceItemRepository
                    .GetAllAsNoTracking()
                    .Where(li => !li.IsDeleted && invoiceIds.Contains(li.SalesInvoiceId))
                    .Include(li => li.SalesInvoice)
                    .Include(li => li.StockBatch)
                    .ThenInclude(b => b.Product)
                    .ToListAsync(cancellationToken);

                foreach (var li in saleLines)
                {
                    var inv = li.SalesInvoice;
                    var pid = li.StockBatch.ProductId;
                    var name = li.StockBatch.Product.Name;
                    Touch(agg, pid, name);

                    var alloc = FinancialSaleMetrics.AllocatedLineDiscount(
                        inv.Subtotal,
                        inv.DiscountAmount,
                        li.Subtotal);
                    var netLine = li.Subtotal - alloc;
                    var unit = EffectiveUnitCostResolver.ResolveForSaleLine(
                        li.UnitEffectiveCostAtSale,
                        li.BatchNominalPurchasePriceAtSale,
                        li.BatchReceivedQuantityAtSale,
                        li.BatchBonusQuantityAtSale);
                    var cogs = unit * li.Quantity;

                    var a = agg[pid];
                    a.SaleNetRevenue += netLine;
                    a.SaleCogs += cogs;
                    a.SoldQuantity += li.Quantity;
                }
            }

            var returnsInPeriod = await _salesReturnRepository
                .GetAllAsNoTracking()
                .Include(sr => sr.SalesInvoice)
                .Include(sr => sr.Items)
                .ThenInclude(i => i.StockBatch)
                .ThenInclude(b => b.Product)
                .Where(sr =>
                    !sr.IsDeleted &&
                    !sr.SalesInvoice.IsDeleted &&
                    sr.SalesInvoice.BranchId == branchId &&
                    sr.CreatedAt >= request.FromDate &&
                    sr.CreatedAt <= request.ToDate)
                .ToListAsync(cancellationToken);

            var (_, recoveryByItem) = await SalesReturnCogsRecoveryEvaluator.EvaluateAsync(
                returnsInPeriod,
                _salesInvoiceItemRepository,
                cancellationToken);

            var linkedLineIds = returnsInPeriod
                .SelectMany(r => r.Items.Where(i => !i.IsDeleted && i.SalesInvoiceItemId.HasValue))
                .Select(i => i.SalesInvoiceItemId!.Value)
                .Distinct()
                .ToList();

            var lineToProduct = linkedLineIds.Count == 0
                ? new Dictionary<Guid, Guid>()
                : await _salesInvoiceItemRepository
                    .GetAllAsNoTracking()
                    .Where(l => linkedLineIds.Contains(l.Id))
                    .Select(l => new { l.Id, ProductId = l.StockBatch.ProductId })
                    .ToDictionaryAsync(x => x.Id, x => x.ProductId, cancellationToken);

            foreach (var ret in returnsInPeriod)
            {
                foreach (var ri in ret.Items.Where(i => !i.IsDeleted))
                {
                    var refund = ri.Quantity * ri.UnitPrice;
                    var recovery = recoveryByItem.GetValueOrDefault(ri.Id);
                    var pid = ri.SalesInvoiceItemId is Guid lid && lineToProduct.TryGetValue(lid, out var pFromLine)
                        ? pFromLine
                        : ri.StockBatch.ProductId;
                    var name = ri.StockBatch.Product.Name;

                    Touch(agg, pid, name);
                    var a = agg[pid];
                    a.ReturnRefunds += refund;
                    a.ReturnRecovery += recovery;
                    a.ReturnedQuantity += ri.Quantity;
                }
            }

            var rows = agg.Values
                .Select(a => new ProductProfitRowDto
                {
                    ProductId = a.ProductId,
                    ProductName = a.Name,
                    SoldQuantity = a.SoldQuantity,
                    ReturnedQuantity = a.ReturnedQuantity,
                    NetSales = a.SaleNetRevenue - a.ReturnRefunds,
                    NetCostOfGoodsSold = a.SaleCogs - a.ReturnRecovery,
                    GrossProfit = (a.SaleNetRevenue - a.ReturnRefunds) - (a.SaleCogs - a.ReturnRecovery)
                })
                .ToList();

            IEnumerable<ProductProfitRowDto> ordered = worst
                ? rows.OrderBy(r => r.GrossProfit).ThenBy(r => r.ProductName)
                : rows.OrderByDescending(r => r.GrossProfit).ThenBy(r => r.ProductName);

            var taken = ordered.Take(take).ToList();

            return new ProductProfitRankingReportDto
            {
                FromUtc = request.FromDate,
                ToUtc = request.ToDate,
                Rank = worst ? "WorstProfit" : "BestProfit",
                Take = take,
                Rows = taken
            };
        }

        private static void Touch(Dictionary<Guid, ProductAgg> agg, Guid productId, string name)
        {
            if (agg.TryGetValue(productId, out var a))
            {
                if (string.IsNullOrWhiteSpace(a.Name) && !string.IsNullOrWhiteSpace(name))
                    a.Name = name;
            }
            else
            {
                agg[productId] = new ProductAgg(productId, string.IsNullOrWhiteSpace(name) ? "—" : name);
            }
        }

        private sealed class ProductAgg
        {
            public ProductAgg(Guid productId, string name)
            {
                ProductId = productId;
                Name = name;
            }

            public Guid ProductId { get; }
            public string Name { get; set; }
            public decimal SaleNetRevenue { get; set; }
            public decimal SaleCogs { get; set; }
            public int SoldQuantity { get; set; }
            public decimal ReturnRefunds { get; set; }
            public decimal ReturnRecovery { get; set; }
            public int ReturnedQuantity { get; set; }
        }
    }
}
