using MediatR;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Common.Interfaces;
using Pharmacy.Application.DTOs.Products;
using Pharmacy.Domain.Entities.Catalog;
using Pharmacy.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Products.Queries.GetProductById
{
    public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto>
    {
        private readonly IRepository<Product> _productRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetProductByIdQueryHandler(
            IRepository<Product> productRepository,
            ICurrentUserService currentUserService)
        {
            _productRepository = productRepository;
            _currentUserService = currentUserService;
        }

        public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (_currentUserService.BranchId is null)
                throw new UnauthorizedException("لا يوجد فرع مرتبط بالمستخدم الحالي");
            var branchId = _currentUserService.BranchId.Value;

            var product = await _productRepository
                    .GetAllAsNoTracking()
                    .Where(p => p.Id == request.ProductId &&
                                !p.IsDeleted &&
                                p.BranchId == branchId
                    )
                    .Select(p => new ProductDto
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
                         TotalQuantity = p.StockBatches
                            .Where(b => b.BranchId == branchId)
                            .Sum(b => b.AvailableQuantity),

                        ExpiredQuantity = p.StockBatches
                            .Where(b =>
                                b.BranchId == branchId &&
                                b.AvailableQuantity > 0 &&
                                b.ExpiryDate <= DateTime.UtcNow)
                            .Sum(b => b.AvailableQuantity),

                        SellableQuantity = p.StockBatches
                            .Where(b =>
                                b.BranchId == branchId &&
                                b.AvailableQuantity > 0 &&
                                b.ExpiryDate > DateTime.UtcNow)
                            .Sum(b => b.AvailableQuantity)
                                            })
                    .FirstOrDefaultAsync(cancellationToken);

            if (product is null)
                throw new NotFoundException("Product", request.ProductId);

            return product;
        }
    }
}
