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

namespace Pharmacy.Application.Features.Sales.Queries.GetSalesInvoiceById
{
    public class GetSalesInvoiceByIdQueryHandler : IRequestHandler<GetSalesInvoiceByIdQuery, SalesInvoiceDetailsDto>
    {
        private readonly IRepository<SalesInvoice> _salesInvoiceRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetSalesInvoiceByIdQueryHandler(
            IRepository<SalesInvoice> salesInvoiceRepository,
            ICurrentUserService currentUserService)
        {
            _salesInvoiceRepository = salesInvoiceRepository;
            _currentUserService = currentUserService;
        }

        public async Task<SalesInvoiceDetailsDto> Handle(GetSalesInvoiceByIdQuery request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (_currentUserService.BranchId is null)
                throw new UnauthorizedException("لا يوجد فرع مرتبط بالمستخدم");

            var invoice = await _salesInvoiceRepository
                .GetAllAsNoTracking()
                .Include(si => si.Customer)
                .Include(si => si.User)
                .FirstOrDefaultAsync(
                    si => si.Id == request.SalesInvoiceId &&
                          !si.IsDeleted &&
                          si.BranchId == _currentUserService.BranchId.Value,
                    cancellationToken);

            if (invoice is null)
                throw new NotFoundException("SalesInvoice", request.SalesInvoiceId);

            return new SalesInvoiceDetailsDto
            {
                SalesInvoiceId = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                GrandTotal = invoice.GrandTotal,
                PaidAmount = invoice.PaidAmount,
                RemainingAmount = invoice.RemainingAmount,
                Status = invoice.Status.ToString()
            };
        }
    }
}
