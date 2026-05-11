using MediatR;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Common.Inventory;
using Pharmacy.Application.Common.Interfaces;
using Pharmacy.Application.DTOs.Products;
using Pharmacy.Domain.Entities.Catalog;
using Pharmacy.Domain.Exceptions;

namespace Pharmacy.Application.Features.Products.Queries.GetProductById
{
    public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto>
    {
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<StockBatch> _stockBatchRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetProductByIdQueryHandler(
            IRepository<Product> productRepository,
            IRepository<StockBatch> stockBatchRepository,
            ICurrentUserService currentUserService)
        {
            _productRepository = productRepository;
            _stockBatchRepository = stockBatchRepository;
            _currentUserService = currentUserService;
        }

        public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (_currentUserService.BranchId is null)
                throw new UnauthorizedException("لا يوجد فرع مرتبط بالمستخدم الحالي");

            var branchId = _currentUserService.BranchId.Value;
            var asOfUtc = DateTime.UtcNow;

            var product = await _productRepository
                .GetAllAsNoTracking()
                .Where(p => p.Id == request.ProductId &&
                            !p.IsDeleted &&
                            p.BranchId == branchId)
                .Select(p => new ProductDto
                {
                    ProductId = p.Id,
                    Name = p.Name,
                    ScientificName = p.ScientificName,
                    Barcode = p.Barcode,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    SellingPrice = p.SellingPrice,
                    PricingType = p.PricingType,
                    PurchasePrice = p.ReferencePurchasePrice,
                    CalculatedUnitProfit = p.ReferencePurchasePrice == null
                        ? null
                        : (decimal?)(p.SellingPrice - p.ReferencePurchasePrice.Value),
                    DefaultSupplierId = p.DefaultSupplierId,
                    BranchId = p.BranchId,
                    TotalAvailableQuantity = 0,
                    SellableQuantity = 0,
                    ExpiredQuantity = 0
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (product is null)
                throw new NotFoundException("Product", request.ProductId);

            var aggregate = await ProductAvailableQuantity.GetStockAggregatesByProductIds(
                    _stockBatchRepository.GetAllAsNoTracking(),
                    branchId,
                    asOfUtc,
                    new[] { request.ProductId })
                .FirstOrDefaultAsync(cancellationToken);

            if (aggregate is not null)
            {
                product.TotalAvailableQuantity = aggregate.TotalAvailableQuantity;
                product.SellableQuantity = aggregate.SellableQuantity;
                product.ExpiredQuantity = aggregate.ExpiredQuantity;
            }

            return product;
        }
    }
}
