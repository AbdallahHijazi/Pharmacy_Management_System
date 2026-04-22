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

namespace Pharmacy.Application.Features.Sales.Queries.GetSalesReturnItems
{
    public class GetSalesReturnItemsQueryHandler : IRequestHandler<GetSalesReturnItemsQuery, List<SalesReturnItemDto>>
    {
        private readonly IRepository<SalesReturnItem> _salesReturnItemRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetSalesReturnItemsQueryHandler(
            IRepository<SalesReturnItem> salesReturnItemRepository,
            ICurrentUserService currentUserService)
        {
            _salesReturnItemRepository = salesReturnItemRepository;
            _currentUserService = currentUserService;
        }

        public async Task<List<SalesReturnItemDto>> Handle(GetSalesReturnItemsQuery request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (_currentUserService.BranchId is null)
                throw new UnauthorizedException("لا يوجد فرع مرتبط بالمستخدم");

            var items = await _salesReturnItemRepository
                .GetAll()
                .Include(i => i.SalesReturn)
                .ThenInclude(sr => sr.SalesInvoice)
                .Where(i => !i.IsDeleted &&
                            i.SalesReturnId == request.SalesReturnId &&
                            i.SalesReturn.SalesInvoice.BranchId == _currentUserService.BranchId.Value)
                .Select(i => new SalesReturnItemDto
                {
                    SalesReturnItemId = i.Id,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Subtotal = i.Quantity * i.UnitPrice
                })
                .ToListAsync(cancellationToken);

            return items;
        }
    }
}
