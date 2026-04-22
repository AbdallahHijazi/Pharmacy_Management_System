using MediatR;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Common.Interfaces;
using Pharmacy.Application.DTOs.Reports;
using Pharmacy.Domain.Entities.Sales;
using Pharmacy.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Reports.Queries.GetSalesReport
{
    public class GetSalesReportQueryHandler : IRequestHandler<GetSalesReportQuery, SalesReportDto>
    {
        private readonly IRepository<SalesInvoice> _salesInvoiceRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetSalesReportQueryHandler(
            IRepository<SalesInvoice> salesInvoiceRepository,
            ICurrentUserService currentUserService)
        {
            _salesInvoiceRepository = salesInvoiceRepository;
            _currentUserService = currentUserService;
        }

        public async Task<SalesReportDto> Handle(GetSalesReportQuery request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (_currentUserService.BranchId is null)
                throw new UnauthorizedException("لا يوجد فرع مرتبط بالمستخدم");

            if (request.FromDate > request.ToDate)
                throw new BadRequestException("تاريخ البداية يجب أن يكون قبل تاريخ النهاية");

            var branchId = _currentUserService.BranchId.Value;

            var query = _salesInvoiceRepository
                .GetAllAsNoTracking()
                .Where(si => !si.IsDeleted &&
                             si.BranchId == branchId &&
                             si.CreatedAt >= request.FromDate &&
                             si.CreatedAt <= request.ToDate);

            if (request.CustomerId.HasValue)
            {
                query = query.Where(si => si.CustomerId == request.CustomerId.Value);
            }

            var totalInvoices = await query.CountAsync(cancellationToken);

            var totalSales = await query
                .Select(x => (decimal?)x.GrandTotal)
                .SumAsync(cancellationToken) ?? 0;

            var totalPaid = await query
                .Select(x => (decimal?)x.PaidAmount)
                .SumAsync(cancellationToken) ?? 0;

            var totalRemaining = await query
                .Select(x => (decimal?)x.RemainingAmount)
                .SumAsync(cancellationToken) ?? 0;

            return new SalesReportDto
            {
                TotalInvoices = totalInvoices,
                TotalSales = totalSales,
                TotalPaid = totalPaid,
                TotalRemaining = totalRemaining
            };
        }
    }
}
