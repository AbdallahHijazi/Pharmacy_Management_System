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

namespace Pharmacy.Application.Features.Sales.Queries.GetSalesReturnById
{
    public class GetSalesReturnByIdQueryHandler : IRequestHandler<GetSalesReturnByIdQuery, SalesReturnDetailsDto>
    {
        private readonly IRepository<SalesReturn> _salesReturnRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetSalesReturnByIdQueryHandler(
            IRepository<SalesReturn> salesReturnRepository,
            ICurrentUserService currentUserService)
        {
            _salesReturnRepository = salesReturnRepository;
            _currentUserService = currentUserService;
        }

        public async Task<SalesReturnDetailsDto> Handle(GetSalesReturnByIdQuery request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (_currentUserService.BranchId is null)
                throw new UnauthorizedException("لا يوجد فرع مرتبط بالمستخدم");

            var salesReturn = await _salesReturnRepository
                .GetAllAsNoTracking()
                .Include(sr => sr.SalesInvoice)
                .FirstOrDefaultAsync(
                    sr => sr.Id == request.SalesReturnId &&
                          !sr.IsDeleted &&
                          sr.SalesInvoice.BranchId == _currentUserService.BranchId.Value,
                    cancellationToken);

            if (salesReturn is null)
                throw new NotFoundException("SalesReturn", request.SalesReturnId);

            return new SalesReturnDetailsDto
            {
                SalesReturnId = salesReturn.Id,
                SalesInvoiceId = salesReturn.SalesInvoiceId,
                InvoiceNumber = salesReturn.SalesInvoice.InvoiceNumber,
                UserId = salesReturn.UserId,
                RefundAmount = salesReturn.RefundAmount,
                Reason = salesReturn.Reason,
                CreatedAt = salesReturn.CreatedAt,
                BranchId = salesReturn.SalesInvoice.BranchId
            };
        }
    }
}
