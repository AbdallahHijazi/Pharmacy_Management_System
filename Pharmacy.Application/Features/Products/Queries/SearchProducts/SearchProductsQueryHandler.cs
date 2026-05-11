using MediatR;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Common.Inventory;
using Pharmacy.Application.Common.Interfaces;
using Pharmacy.Application.DTOs.Products;
using Pharmacy.Domain.Entities.Catalog;
using Pharmacy.Domain.Exceptions;

namespace Pharmacy.Application.Features.Products.Queries.SearchProducts
{
    public class SearchProductsQueryHandler : IRequestHandler<SearchProductsQuery, List<ProductListItemDto>>
    {
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<StockBatch> _stockBatchRepository;
        private readonly ICurrentUserService _currentUserService;

        public SearchProductsQueryHandler(
            IRepository<Product> productRepository,
            IRepository<StockBatch> stockBatchRepository,
            ICurrentUserService currentUserService)
        {
            _productRepository = productRepository;
            _stockBatchRepository = stockBatchRepository;
            _currentUserService = currentUserService;
        }

        public async Task<List<ProductListItemDto>> Handle(SearchProductsQuery request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (_currentUserService.BranchId is null)
                throw new UnauthorizedException("لا يوجد فرع مرتبط بالمستخدم الحالي");

            var query = request.Query?.Trim();

            if (string.IsNullOrWhiteSpace(query))
                return new List<ProductListItemDto>();

            var normalizedQuery = query.ToLower();
            var branchId = _currentUserService.BranchId.Value;
            var asOfUtc = DateTime.UtcNow;

            var items = await _productRepository
                .GetAllAsNoTracking()
                .Include(p => p.Category)
                .Where(p => !p.IsDeleted &&
                            p.BranchId == branchId &&
                            (
                                p.Name.ToLower().Contains(normalizedQuery) ||
                                p.ScientificName.ToLower().Contains(normalizedQuery) ||
                                p.Barcode.ToLower().Contains(normalizedQuery)
                            ))
                .OrderBy(p => p.Name)
                .Take(20)
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
                    TotalQuantity = 0,
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

                    item.TotalQuantity = a.TotalAvailableQuantity;
                    item.SellableQuantity = a.SellableQuantity;
                    item.ExpiredQuantity = a.ExpiredQuantity;
                }
            }

            return items;
        }
    }
}
