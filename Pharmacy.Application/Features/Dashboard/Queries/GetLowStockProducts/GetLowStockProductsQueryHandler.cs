using MediatR;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Common.Interfaces;
using Pharmacy.Application.DTOs.Dashboard;
using Pharmacy.Domain.Entities.Catalog;
using Pharmacy.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Dashboard.Queries.GetLowStockProducts
{
    public class GetLowStockProductsQueryHandler : IRequestHandler<GetLowStockProductsQuery, List<LowStockProductDto>>
    {
        private readonly IRepository<StockBatch> _stockBatchRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetLowStockProductsQueryHandler(
            IRepository<StockBatch> stockBatchRepository,
            ICurrentUserService currentUserService)
        {
            _stockBatchRepository = stockBatchRepository;
            _currentUserService = currentUserService;
        }

        public async Task<List<LowStockProductDto>> Handle(GetLowStockProductsQuery request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (_currentUserService.BranchId is null)
                throw new UnauthorizedException("لا يوجد فرع مرتبط بالمستخدم");

            var branchId = _currentUserService.BranchId.Value;

            var products = await _stockBatchRepository
                .GetAllAsNoTracking()
                .Include(sb => sb.Product)
                .Where(sb => !sb.IsDeleted &&
                             sb.BranchId == branchId &&
                             sb.AvailableQuantity > 0)
                .GroupBy(sb => new { sb.ProductId, sb.Product.Name })
                .Select(g => new LowStockProductDto
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    TotalAvailableQuantity = g.Sum(x => x.AvailableQuantity),
                    BatchesCount = g.Count()
                })
                .Where(x => x.TotalAvailableQuantity <= 10)
                .OrderBy(x => x.TotalAvailableQuantity)
                .ThenBy(x => x.ProductName)
                .ToListAsync(cancellationToken);

            return products;
        }
    }
}
