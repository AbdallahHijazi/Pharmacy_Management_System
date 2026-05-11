using MediatR;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Common.Interfaces;
using Pharmacy.Application.DTOs.Products;
using Pharmacy.Domain.Entities.Catalog;
using Pharmacy.Domain.Entities.Partners;
using Pharmacy.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Products.Commands.CreateProduct
{
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDetailsDto>
    {
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductCategory> _categoryRepository;
        private readonly IRepository<Supplier> _supplierRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public CreateProductCommandHandler(
            IRepository<Product> productRepository,
            IRepository<ProductCategory> categoryRepository,
            IRepository<Supplier> supplierRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _supplierRepository = supplierRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<ProductDetailsDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (_currentUserService.BranchId is null)
                throw new UnauthorizedException("لا يوجد فرع مرتبط بالمستخدم الحالي");

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new BadRequestException("اسم المنتج مطلوب");

            if (string.IsNullOrWhiteSpace(request.Barcode))
                throw new BadRequestException("الباركود مطلوب");

            if (request.SellingPrice < 0)
                throw new BadRequestException("سعر البيع يجب أن يكون أكبر من أو يساوي صفر");

            var category = await _categoryRepository
                .GetAll()
                .FirstOrDefaultAsync(
                    c => c.Id == request.CategoryId && !c.IsDeleted,
                    cancellationToken);

            if (category is null)
                throw new NotFoundException("Category", request.CategoryId);

            Supplier? supplier = null;

            if (request.DefaultSupplierId.HasValue)
            {
                supplier = await _supplierRepository
                    .GetAll()
                    .FirstOrDefaultAsync(
                        s => s.Id == request.DefaultSupplierId.Value &&
                             !s.IsDeleted &&
                             s.BranchId == _currentUserService.BranchId.Value,
                        cancellationToken);

                if (supplier is null)
                    throw new NotFoundException("Supplier", request.DefaultSupplierId.Value);
            }

            var normalizedBarcode = request.Barcode.Trim();

            var barcodeExists = await _productRepository
                .GetAll()
                .AnyAsync(
                    p => !p.IsDeleted && p.Barcode.ToLower() == normalizedBarcode.ToLower(),
                    cancellationToken);

            if (barcodeExists)
                throw new StatusAlreadyExistsException(request.Barcode);

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = request.Name.Trim(),
                ScientificName = request.ScientificName?.Trim() ?? string.Empty,
                Barcode = normalizedBarcode,
                CategoryId = request.CategoryId,
                SellingPrice = request.SellingPrice,
                DefaultSupplierId = request.DefaultSupplierId,
                BranchId = _currentUserService.BranchId.Value,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = _currentUserService.UserId.Value,
                IsDeleted = false
            };

            _productRepository.Add(product);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ProductDetailsDto
            {
                ProductId = product.Id,
                Name = product.Name,
                ScientificName = product.ScientificName,
                Barcode = product.Barcode,
                CategoryId = product.CategoryId,
                CategoryName = category.Name,
                SellingPrice = product.SellingPrice,
                DefaultSupplierId = product.DefaultSupplierId,
                BranchId = product.BranchId,
                TotalAvailableQuantity = 0,
                SellableQuantity = 0,
                ExpiredQuantity = 0
            };
        }
    }
}
