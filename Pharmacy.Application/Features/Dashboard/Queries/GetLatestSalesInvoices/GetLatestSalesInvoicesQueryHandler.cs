using MediatR;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Common.Interfaces;
using Pharmacy.Application.DTOs.Dashboard;
using Pharmacy.Domain.Entities.Sales;
using Pharmacy.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Dashboard.Queries.GetLatestSalesInvoices
{
    public class GetLatestSalesInvoicesQueryHandler : IRequestHandler<GetLatestSalesInvoicesQuery, List<LatestSalesInvoiceDto>>
    {
        private readonly IRepository<SalesInvoice> _salesInvoiceRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetLatestSalesInvoicesQueryHandler(
            IRepository<SalesInvoice> salesInvoiceRepository,
            ICurrentUserService currentUserService)
        {
            _salesInvoiceRepository = salesInvoiceRepository;
            _currentUserService = currentUserService;
        }

        public async Task<List<LatestSalesInvoiceDto>> Handle(GetLatestSalesInvoicesQuery request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (_currentUserService.BranchId is null)
                throw new UnauthorizedException("لا يوجد فرع مرتبط بالمستخدم");

            var branchId = _currentUserService.BranchId.Value;

            var invoices = await _salesInvoiceRepository
                .GetAllAsNoTracking()
                .Include(si => si.Customer)
                .Where(si => !si.IsDeleted && si.BranchId == branchId)
                .OrderByDescending(si => si.CreatedAt)
                .Take(10)
                .Select(si => new LatestSalesInvoiceDto
                {
                    SalesInvoiceId = si.Id,
                    InvoiceNumber = si.InvoiceNumber,
                    CustomerName = si.Customer != null ? si.Customer.FullName : string.Empty,
                    GrandTotal = si.GrandTotal,
                    Status = si.Status.ToString(),
                    CreatedAt = si.CreatedAt
                })
                .ToListAsync(cancellationToken);

            return invoices;
        }
    }
}
