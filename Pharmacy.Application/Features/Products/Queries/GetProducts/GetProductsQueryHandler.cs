using MediatR;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Common.Inventory;
using Pharmacy.Application.Common.Interfaces;
using Pharmacy.Application.Common.Models;
using Pharmacy.Application.DTOs.Products;
using Pharmacy.Domain.Entities.Catalog;
using Pharmacy.Domain.Exceptions;

namespace Pharmacy.Application.Features.Products.Queries.GetProducts
{
    public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, PagedResult<ProductListItemDto>>
    {
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<StockBatch> _stockBatchRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetProductsQueryHandler(
            IRepository<Product> productRepository,
            IRepository<StockBatch> stockBatchRepository,
            ICurrentUserService currentUserService)
        {
            _productRepository = productRepository;
            _stockBatchRepository = stockBatchRepository;
            _currentUserService = currentUserService;
        }

        public async Task<PagedResult<ProductListItemDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
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
            var asOfUtc = DateTime.UtcNow;

            var query = _productRepository
                .GetAllAsNoTracking()
                .Include(p => p.Category)
                .Where(p => !p.IsDeleted && p.BranchId == branchId);

            query = (request.SortBy?.ToLower(), request.SortDirection?.ToLower()) switch
            {
                ("name", "desc") => query.OrderByDescending(p => p.Name),
                ("scientificname", "desc") => query.OrderByDescending(p => p.ScientificName),
                ("barcode", "desc") => query.OrderByDescending(p => p.Barcode),
                ("sellingprice", "desc") => query.OrderByDescending(p => p.SellingPrice),

                ("scientificname", _) => query.OrderBy(p => p.ScientificName),
                ("barcode", _) => query.OrderBy(p => p.Barcode),
                ("sellingprice", _) => query.OrderBy(p => p.SellingPrice),

                _ => query.OrderBy(p => p.Name)
            };

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(p => new ProductListItemDto
                {
                    ProductId = p.Id,
                    Name = p.Name,
                    ScientificName = p.ScientificName,
                    Barcode = p.Barcode,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    SellingPrice = p.SellingPrice,
                    DefaultSupplierId = p.DefaultSupplierId,
                    BranchId = p.BranchId,
                    TotalAvailableQuantity = 0,
                    SellableQuantity = 0,
                    ExpiredQuantity = 0
                })
                .ToListAsync(cancellationToken);

            var productIds = items.Select(i => i.ProductId).Distinct().ToList();
            if (productIds.Count > 0)
            {
                var aggregates = await ProductAvailableQuantity.GetStockAggregatesByProductIds(
                        _stockBatchRepository.GetAllAsNoTracking(),
                        branchId,
                        asOfUtc,
                        productIds)
                    .ToListAsync(cancellationToken);

                var map = aggregates.ToDictionary(a => a.ProductId);
                foreach (var item in items)
                {
                    if (!map.TryGetValue(item.ProductId, out var a))
                        continue;

                    item.TotalAvailableQuantity = a.TotalAvailableQuantity;
                    item.SellableQuantity = a.SellableQuantity;
                    item.ExpiredQuantity = a.ExpiredQuantity;
                }
            }

            return new PagedResult<ProductListItemDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
