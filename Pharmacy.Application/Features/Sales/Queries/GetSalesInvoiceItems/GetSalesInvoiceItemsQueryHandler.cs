using MediatR;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Common.Interfaces;
using Pharmacy.Application.DTOs.Sales;
using Pharmacy.Domain.Entities.Sales;
using Pharmacy.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Sales.Queries.GetSalesInvoiceItems
{
    public class GetSalesInvoiceItemsQueryHandler : IRequestHandler<GetSalesInvoiceItemsQuery, List<SalesInvoiceItemDto>>
    {
        private readonly IRepository<SalesInvoiceItem> _salesInvoiceItemRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetSalesInvoiceItemsQueryHandler(
            IRepository<SalesInvoiceItem> salesInvoiceItemRepository,
            ICurrentUserService currentUserService)
        {
            _salesInvoiceItemRepository = salesInvoiceItemRepository;
            _currentUserService = currentUserService;
        }

        public async Task<List<SalesInvoiceItemDto>> Handle(GetSalesInvoiceItemsQuery request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (_currentUserService.BranchId is null)
                throw new UnauthorizedException("لا يوجد فرع مرتبط بالمستخدم");

            var items = await _salesInvoiceItemRepository
                .GetAllAsNoTracking()
                .Include(i => i.StockBatch)
                .ThenInclude(sb => sb.Product)
                .Include(i => i.SalesInvoice)
                .Where(i => !i.IsDeleted &&
                            i.SalesInvoiceId == request.SalesInvoiceId &&
                            i.SalesInvoice.BranchId == _currentUserService.BranchId.Value)
                .Select(i => new SalesInvoiceItemDto
                {
                    SalesInvoiceItemId = i.Id,
                    StockBatchId = i.StockBatchId,
                    ProductId = i.StockBatch.ProductId,
                    ProductName = i.StockBatch.Product.Name,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Subtotal = i.Subtotal,
                    UnitEffectiveCostAtSale = i.UnitEffectiveCostAtSale,
                    BatchNominalPurchasePriceAtSale = i.BatchNominalPurchasePriceAtSale,
                    BatchReceivedQuantityAtSale = i.BatchReceivedQuantityAtSale,
                    BatchBonusQuantityAtSale = i.BatchBonusQuantityAtSale
                })
                .ToListAsync(cancellationToken);

            return items;
        }
    }
}
