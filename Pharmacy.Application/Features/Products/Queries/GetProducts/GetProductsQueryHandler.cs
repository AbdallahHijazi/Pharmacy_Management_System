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

namespace Pharmacy.Application.Features.Products.Queries.GetProducts
{
    public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, List<ProductListItemDto>>
    {
        private readonly IRepository<Product> _productRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetProductsQueryHandler(
            IRepository<Product> productRepository,
            ICurrentUserService currentUserService)
        {
            _productRepository = productRepository;
            _currentUserService = currentUserService;
        }

        public async Task<List<ProductListItemDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (_currentUserService.BranchId is null)
                throw new UnauthorizedException("لا يوجد فرع مرتبط بالمستخدم الحالي");

            var products = await _productRepository
                .GetAll()
                .Include(p => p.Category)
                .Where(p => !p.IsDeleted && p.BranchId == _currentUserService.BranchId.Value)
                .OrderBy(p => p.Name)
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
                    BranchId = p.BranchId
                })
                .ToListAsync(cancellationToken);

            return products;
        }
    }
}
