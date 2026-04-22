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

namespace Pharmacy.Application.Features.Purchases.Queries.GetPurchaseReturnItems
{
    public class GetPurchaseReturnItemsQueryHandler : IRequestHandler<GetPurchaseReturnItemsQuery, List<PurchaseReturnItemDto>>
    {
        private readonly IRepository<PurchaseReturnItem> _purchaseReturnItemRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetPurchaseReturnItemsQueryHandler(
            IRepository<PurchaseReturnItem> purchaseReturnItemRepository,
            ICurrentUserService currentUserService)
        {
            _purchaseReturnItemRepository = purchaseReturnItemRepository;
            _currentUserService = currentUserService;
        }

        public async Task<List<PurchaseReturnItemDto>> Handle(GetPurchaseReturnItemsQuery request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (_currentUserService.BranchId is null)
                throw new UnauthorizedException("لا يوجد فرع مرتبط بالمستخدم");

            var items = await _purchaseReturnItemRepository
                .GetAllAsNoTracking()
                .Include(i => i.Product)
                .Include(i => i.PurchaseReturn)
                .ThenInclude(pr => pr.PurchaseInvoice)
                .Where(i => !i.IsDeleted &&
                            i.PurchaseReturnId == request.PurchaseReturnId &&
                            i.PurchaseReturn.PurchaseInvoice.BranchId == _currentUserService.BranchId.Value)
                .Select(i => new PurchaseReturnItemDto
                {
                    PurchaseReturnItemId = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.Product.Name,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Subtotal = i.Quantity * i.UnitPrice
                })
                .ToListAsync(cancellationToken);

            return items;
        }
    }
}
