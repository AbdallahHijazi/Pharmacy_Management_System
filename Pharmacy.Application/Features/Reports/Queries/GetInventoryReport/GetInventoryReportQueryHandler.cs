using MediatR;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Common.Interfaces;
using Pharmacy.Application.DTOs.Reports;
using Pharmacy.Domain.Entities.Catalog;
using Pharmacy.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Reports.Queries.GetInventoryReport
{
    public class GetInventoryReportQueryHandler : IRequestHandler<GetInventoryReportQuery, InventoryReportDto>
    {
        private readonly IRepository<StockBatch> _stockBatchRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetInventoryReportQueryHandler(
            IRepository<StockBatch> stockBatchRepository,
            ICurrentUserService currentUserService)
        {
            _stockBatchRepository = stockBatchRepository;
            _currentUserService = currentUserService;
        }

        public async Task<InventoryReportDto> Handle(GetInventoryReportQuery request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (_currentUserService.BranchId is null)
                throw new UnauthorizedException("لا يوجد فرع مرتبط بالمستخدم");

            var branchId = _currentUserService.BranchId.Value;
            var today = DateTime.UtcNow.Date;
            var expiringSoonDate = today.AddDays(30);

            var baseQuery = _stockBatchRepository
                .GetAll()
                .Where(sb => !sb.IsDeleted && sb.BranchId == branchId);

            var totalProductsInStock = await baseQuery
                .Where(sb => sb.AvailableQuantity > 0)
                .Select(sb => sb.ProductId)
                .Distinct()
                .CountAsync(cancellationToken);

            var totalAvailableQuantity = await baseQuery
                .Select(sb => (int?)sb.AvailableQuantity)
                .SumAsync(cancellationToken) ?? 0;

            var lowStockBatchesCount = await baseQuery
                .CountAsync(sb => sb.AvailableQuantity > 0 && sb.AvailableQuantity <= 10, cancellationToken);

            var expiringSoonBatchesCount = await baseQuery
                .CountAsync(sb => sb.AvailableQuantity > 0 &&
                                  sb.ExpiryDate.Date > today &&
                                  sb.ExpiryDate.Date <= expiringSoonDate,
                    cancellationToken);

            var expiredBatchesCount = await baseQuery
                .CountAsync(sb => sb.AvailableQuantity > 0 &&
                                  sb.ExpiryDate.Date <= today,
                    cancellationToken);

            return new InventoryReportDto
            {
                TotalProductsInStock = totalProductsInStock,
                TotalAvailableQuantity = totalAvailableQuantity,
                LowStockBatchesCount = lowStockBatchesCount,
                ExpiringSoonBatchesCount = expiringSoonBatchesCount,
                ExpiredBatchesCount = expiredBatchesCount
            };
        }
    }
}
