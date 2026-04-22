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
    public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDetailsDto>
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

        public async Task<ProductDetailsDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (_currentUserService.BranchId is null)
                throw new UnauthorizedException("لا يوجد فرع مرتبط بالمستخدم الحالي");

            var product = await _productRepository
                .GetAll()
                .Include(p => p.Category)
                .FirstOrDefaultAsync(
                    p => p.Id == request.ProductId &&
                         !p.IsDeleted &&
                         p.BranchId == _currentUserService.BranchId.Value,
                    cancellationToken);

            if (product is null)
                throw new NotFoundException("Product", request.ProductId);

            return new ProductDetailsDto
            {
                ProductId = product.Id,
                Name = product.Name,
                ScientificName = product.ScientificName,
                Barcode = product.Barcode,
                CategoryId = product.CategoryId,
                CategoryName = product.Category.Name,
                SellingPrice = product.SellingPrice,
                DefaultSupplierId = product.DefaultSupplierId,
                BranchId = product.BranchId
            };
        }
    }
}
