using MediatR;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Common.Interfaces;
using Pharmacy.Application.Common.Models;
using Pharmacy.Application.DTOs.Inventory;
using Pharmacy.Domain.Entities.Catalog;
using Pharmacy.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Inventory.Queries.GetStockBatches
{
    public class GetStockBatchesQueryHandler : IRequestHandler<GetStockBatchesQuery, PagedResult<StockBatchListItemDto>>
    {
        private readonly IRepository<StockBatch> _stockBatchRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetStockBatchesQueryHandler(
            IRepository<StockBatch> stockBatchRepository,
            ICurrentUserService currentUserService)
        {
            _stockBatchRepository = stockBatchRepository;
            _currentUserService = currentUserService;
        }

        public async Task<PagedResult<StockBatchListItemDto>> Handle(GetStockBatchesQuery request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (_currentUserService.BranchId is null)
                throw new UnauthorizedException("لا يوجد فرع مرتبط بالمستخدم الحالي");

            if (request.PageNumber <= 0)
                throw new BadRequestException("رقم الصفحة يجب أن يكون أكبر من صفر");

            if (request.PageSize <= 0 || request.PageSize > 100)
                throw new BadRequestException("حجم الصفحة يجب أن يكون بين 1 و 100");

            var branchId = _currentUserService.BranchId.Value;

            var query = _stockBatchRepository
                .GetAllAsNoTracking()
                .Include(sb => sb.Product)
                .Include(sb => sb.Supplier)
                .Where(sb => !sb.IsDeleted && sb.BranchId == branchId);

            query = (request.SortBy?.ToLower(), request.SortDirection?.ToLower()) switch
            {
                ("expirydate", "desc") => query.OrderByDescending(sb => sb.ExpiryDate),
                ("batchnumber", "desc") => query.OrderByDescending(sb => sb.BatchNumber),
                ("availablequantity", "desc") => query.OrderByDescending(sb => sb.AvailableQuantity),
                ("purchaseprice", "desc") => query.OrderByDescending(sb => sb.PurchasePrice),

                ("batchnumber", _) => query.OrderBy(sb => sb.BatchNumber),
                ("availablequantity", _) => query.OrderBy(sb => sb.AvailableQuantity),
                ("purchaseprice", _) => query.OrderBy(sb => sb.PurchasePrice),

                _ => query.OrderBy(sb => sb.ExpiryDate)
            };

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
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

            return new PagedResult<StockBatchListItemDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
