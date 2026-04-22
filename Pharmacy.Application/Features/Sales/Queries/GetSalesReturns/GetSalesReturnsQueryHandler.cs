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

namespace Pharmacy.Application.Features.Sales.Queries.GetSalesReturns
{
    public class GetSalesReturnsQueryHandler : IRequestHandler<GetSalesReturnsQuery, List<SalesReturnListItemDto>>
    {
        private readonly IRepository<SalesReturn> _salesReturnRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetSalesReturnsQueryHandler(
            IRepository<SalesReturn> salesReturnRepository,
            ICurrentUserService currentUserService)
        {
            _salesReturnRepository = salesReturnRepository;
            _currentUserService = currentUserService;
        }

        public async Task<List<SalesReturnListItemDto>> Handle(GetSalesReturnsQuery request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (_currentUserService.BranchId is null)
                throw new UnauthorizedException("لا يوجد فرع مرتبط بالمستخدم");

            var returns = await _salesReturnRepository
                .GetAllAsNoTracking()
                .Include(sr => sr.SalesInvoice)
                .Include(sr => sr.User)
                .Where(sr => !sr.IsDeleted &&
                             sr.SalesInvoice.BranchId == _currentUserService.BranchId.Value)
                .OrderByDescending(sr => sr.CreatedAt)
                .Select(sr => new SalesReturnListItemDto
                {
                    SalesReturnId = sr.Id,
                    SalesInvoiceId = sr.SalesInvoiceId,
                    InvoiceNumber = sr.SalesInvoice.InvoiceNumber,
                    UserId = sr.UserId,
                    UserFullName = sr.User.FullName,
                    RefundAmount = sr.RefundAmount,
                    Reason = sr.Reason,
                    CreatedAt = sr.CreatedAt,
                    BranchId = sr.SalesInvoice.BranchId
                })
                .ToListAsync(cancellationToken);

            return returns;
        }
    }
}
