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

namespace Pharmacy.Application.Features.Sales.Queries.GetSalesInvoices
{
    public class GetSalesInvoicesQueryHandler : IRequestHandler<GetSalesInvoicesQuery, List<SalesInvoiceListItemDto>>
    {
        private readonly IRepository<SalesInvoice> _salesInvoiceRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetSalesInvoicesQueryHandler(
            IRepository<SalesInvoice> salesInvoiceRepository,
            ICurrentUserService currentUserService)
        {
            _salesInvoiceRepository = salesInvoiceRepository;
            _currentUserService = currentUserService;
        }

        public async Task<List<SalesInvoiceListItemDto>> Handle(GetSalesInvoicesQuery request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (_currentUserService.BranchId is null)
                throw new UnauthorizedException("لا يوجد فرع مرتبط بالمستخدم الحالي");

            var invoices = await _salesInvoiceRepository
                .GetAll()
                .Include(si => si.Customer)
                .Include(si => si.User)
                .Where(si => !si.IsDeleted && si.BranchId == _currentUserService.BranchId.Value)
                .OrderByDescending(si => si.CreatedAt)
                .Select(si => new SalesInvoiceListItemDto
                {
                    SalesInvoiceId = si.Id,
                    InvoiceNumber = si.InvoiceNumber,
                    CustomerId = si.CustomerId,
                    CustomerName = si.Customer != null ? si.Customer.FullName : string.Empty,
                    UserId = si.UserId,
                    UserFullName = si.User.FullName,
                    BranchId = si.BranchId,
                    Subtotal = si.Subtotal,
                    DiscountAmount = si.DiscountAmount,
                    TaxAmount = si.TaxAmount,
                    GrandTotal = si.GrandTotal,
                    PaidAmount = si.PaidAmount,
                    RemainingAmount = si.RemainingAmount,
                    PaymentMethod = si.PaymentMethod.ToString(),
                    Status = si.Status.ToString(),
                    CreatedAt = si.CreatedAt
                })
                .ToListAsync(cancellationToken);

            return invoices;
        }
    }
}
