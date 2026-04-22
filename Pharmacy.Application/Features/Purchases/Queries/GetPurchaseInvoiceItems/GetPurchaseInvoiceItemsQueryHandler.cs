using MediatR;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Common.Interfaces;
using Pharmacy.Application.DTOs.Purchases;
using Pharmacy.Domain.Entities.Purchases;
using Pharmacy.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Purchases.Queries.GetPurchaseInvoiceItems
{
    public class GetPurchaseInvoiceItemsQueryHandler : IRequestHandler<GetPurchaseInvoiceItemsQuery, List<PurchaseInvoiceItemDto>>
    {
        private readonly IRepository<PurchaseInvoiceItem> _purchaseInvoiceItemRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetPurchaseInvoiceItemsQueryHandler(
            IRepository<PurchaseInvoiceItem> purchaseInvoiceItemRepository,
            ICurrentUserService currentUserService)
        {
            _purchaseInvoiceItemRepository = purchaseInvoiceItemRepository;
            _currentUserService = currentUserService;
        }

        public async Task<List<PurchaseInvoiceItemDto>> Handle(GetPurchaseInvoiceItemsQuery request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (_currentUserService.BranchId is null)
                throw new UnauthorizedException("لا يوجد فرع مرتبط بالمستخدم");

            var items = await _purchaseInvoiceItemRepository
                .GetAllAsNoTracking()
                .Include(i => i.Product)
                .Include(i => i.PurchaseInvoice)
                .Where(i => !i.IsDeleted &&
                            i.PurchaseInvoiceId == request.PurchaseInvoiceId &&
                            i.PurchaseInvoice.BranchId == _currentUserService.BranchId.Value)
                .Select(i => new PurchaseInvoiceItemDto
                {
                    PurchaseInvoiceItemId = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.Product.Name,
                    BatchNumber = i.BatchNumber,
                    ExpiryDate = i.ExpiryDate,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                })
                .ToListAsync(cancellationToken);

            return items;
        }
    }
}
