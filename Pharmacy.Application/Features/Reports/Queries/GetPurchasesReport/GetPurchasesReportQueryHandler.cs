using MediatR;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Common.Interfaces;
using Pharmacy.Application.DTOs.Reports;
using Pharmacy.Domain.Entities.Purchases;
using Pharmacy.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Reports.Queries.GetPurchasesReport
{
    public class GetPurchasesReportQueryHandler : IRequestHandler<GetPurchasesReportQuery, PurchasesReportDto>
    {
        private readonly IRepository<PurchaseInvoice> _purchaseInvoiceRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetPurchasesReportQueryHandler(
            IRepository<PurchaseInvoice> purchaseInvoiceRepository,
            ICurrentUserService currentUserService)
        {
            _purchaseInvoiceRepository = purchaseInvoiceRepository;
            _currentUserService = currentUserService;
        }

        public async Task<PurchasesReportDto> Handle(GetPurchasesReportQuery request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (_currentUserService.BranchId is null)
                throw new UnauthorizedException("لا يوجد فرع مرتبط بالمستخدم");

            if (request.FromDate > request.ToDate)
                throw new BadRequestException("تاريخ البداية يجب أن يكون قبل تاريخ النهاية");

            var branchId = _currentUserService.BranchId.Value;

            var query = _purchaseInvoiceRepository
                .GetAllAsNoTracking()
                .Where(pi => !pi.IsDeleted &&
                             pi.BranchId == branchId &&
                             pi.CreatedAt >= request.FromDate &&
                             pi.CreatedAt <= request.ToDate);

            if (request.SupplierId.HasValue)
            {
                query = query.Where(pi => pi.SupplierId == request.SupplierId.Value);
            }

            var totalInvoices = await query.CountAsync(cancellationToken);

            var totalPurchases = await query
                .Select(x => (decimal?)x.GrandTotal)
                .SumAsync(cancellationToken) ?? 0m;

            var totalPaid = await query
                .Select(x => (decimal?)x.PaidAmount)
                .SumAsync(cancellationToken) ?? 0m;

            var totalRemaining = await query
                .Select(x => (decimal?)x.RemainingAmount)
                .SumAsync(cancellationToken) ?? 0m;

            return new PurchasesReportDto
            {
                TotalInvoices = totalInvoices,
                TotalPurchases = totalPurchases,
                TotalPaid = totalPaid,
                TotalRemaining = totalRemaining
            };
        }
    }
}
