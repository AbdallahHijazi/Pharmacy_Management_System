using MediatR;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Common.Interfaces;
using Pharmacy.Application.DTOs.Inventory;
using Pharmacy.Domain.Entities.Catalog;
using Pharmacy.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Inventory.Queries.GetLowStockBatches
{
    public class GetLowStockBatchesQueryHandler : IRequestHandler<GetLowStockBatchesQuery, List<StockBatchListItemDto>>
    {
        private readonly IRepository<StockBatch> _stockBatchRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetLowStockBatchesQueryHandler(
            IRepository<StockBatch> stockBatchRepository,
            ICurrentUserService currentUserService)
        {
            _stockBatchRepository = stockBatchRepository;
            _currentUserService = currentUserService;
        }

        public async Task<List<StockBatchListItemDto>> Handle(GetLowStockBatchesQuery request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (_currentUserService.BranchId is null)
                throw new UnauthorizedException("لا يوجد فرع مرتبط بالمستخدم الحالي");

            var branchId = _currentUserService.BranchId.Value;

            var stockBatches = await _stockBatchRepository
                .GetAllAsNoTracking()
                .Include(sb => sb.Product)
                .Include(sb => sb.Supplier)
                .Where(sb => !sb.IsDeleted &&
                             sb.BranchId == branchId &&
                             sb.AvailableQuantity > 0 &&
                             sb.AvailableQuantity <= 10)
                .OrderBy(sb => sb.AvailableQuantity)
                .ThenBy(sb => sb.ExpiryDate)
                .Select(sb => new StockBatchListItemDto
                {
                    StockBatchId = sb.Id,
                    ProductId = sb.ProductId,
                    ProductName = sb.Product.Name,
                    LotNumber = sb.BatchNumber,
                    ExpiryDate = sb.ExpiryDate,
                    PurchasePrice = sb.PurchasePrice,
                    ReceivedQuantity = sb.ReceivedQuantity,
                    BonusQuantity = sb.BonusQuantity,
                    AvailableQuantity = sb.AvailableQuantity,
                    SupplierId = sb.SupplierId,
                    SupplierName = sb.Supplier.Name,
                    BranchId = sb.BranchId
                })
                .ToListAsync(cancellationToken);

            return stockBatches;
        }
    }
}
