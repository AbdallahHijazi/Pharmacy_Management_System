using MediatR;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Common.Interfaces;
using Pharmacy.Application.DTOs.Inventory;
using Pharmacy.Domain.Entities.Catalog;
using Pharmacy.Domain.Entities.Partners;
using Pharmacy.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Inventory.Commands.CreateStockBatch
{
    public class CreateStockBatchCommandHandler : IRequestHandler<CreateStockBatchCommand, StockBatchDetailsDto>
    {
        private readonly IRepository<StockBatch> _stockBatchRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<Supplier> _supplierRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public CreateStockBatchCommandHandler(
            IRepository<StockBatch> stockBatchRepository,
            IRepository<Product> productRepository,
            IRepository<Supplier> supplierRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _stockBatchRepository = stockBatchRepository;
            _productRepository = productRepository;
            _supplierRepository = supplierRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<StockBatchDetailsDto> Handle(CreateStockBatchCommand request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (_currentUserService.BranchId is null)
                throw new UnauthorizedException("لا يوجد فرع مرتبط بالمستخدم الحالي");

            if (request.ProductId == Guid.Empty)
                throw new BadRequestException("المنتج مطلوب");

            if (request.SupplierId == Guid.Empty)
                throw new BadRequestException("المورد مطلوب");

            if (string.IsNullOrWhiteSpace(request.BatchNumber))
                throw new BadRequestException("رقم التشغيلة مطلوب");

            if (request.PurchasePrice < 0)
                throw new BadRequestException("سعر الشراء يجب أن يكون أكبر من أو يساوي صفر");

            if (request.ReceivedQuantity < 0)
                throw new BadRequestException("الكمية المستلمة يجب أن تكون أكبر من أو تساوي صفر");

            if (request.AvailableQuantity < 0)
                throw new BadRequestException("الكمية المتاحة يجب أن تكون أكبر من أو تساوي صفر");

            if (request.AvailableQuantity > request.ReceivedQuantity)
                throw new BadRequestException("الكمية المتاحة لا يمكن أن تكون أكبر من الكمية المستلمة");

            var product = await _productRepository
                .GetAll()
                .FirstOrDefaultAsync(
                    p => p.Id == request.ProductId &&
                         !p.IsDeleted &&
                         p.BranchId == _currentUserService.BranchId.Value,
                    cancellationToken);

            if (product is null)
                throw new NotFoundException("Product", request.ProductId);

            var supplier = await _supplierRepository
                .GetAll()
                .FirstOrDefaultAsync(
                    s => s.Id == request.SupplierId &&
                         !s.IsDeleted &&
                         s.BranchId == _currentUserService.BranchId.Value,
                    cancellationToken);

            if (supplier is null)
                throw new NotFoundException("Supplier", request.SupplierId);

            var normalizedBatchNumber = request.BatchNumber.Trim();

            var exists = await _stockBatchRepository
                .GetAll()
                .AnyAsync(
                    sb => !sb.IsDeleted &&
                          sb.ProductId == request.ProductId &&
                          sb.BatchNumber.ToLower() == normalizedBatchNumber.ToLower(),
                    cancellationToken);

            if (exists)
                throw new StatusAlreadyExistsException(request.BatchNumber);

            var stockBatch = new StockBatch
            {
                Id = Guid.NewGuid(),
                ProductId = request.ProductId,
                BatchNumber = normalizedBatchNumber,
                ExpiryDate = request.ExpiryDate,
                PurchasePrice = request.PurchasePrice,
                ReceivedQuantity = request.ReceivedQuantity,
                AvailableQuantity = request.AvailableQuantity,
                SupplierId = request.SupplierId,
                BranchId = _currentUserService.BranchId.Value,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = _currentUserService.UserId.Value,
                IsDeleted = false
            };

            _stockBatchRepository.Add(stockBatch);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new StockBatchDetailsDto
            {
                StockBatchId = stockBatch.Id,
                ProductId = stockBatch.ProductId,
                ProductName = product.Name,
                BatchNumber = stockBatch.BatchNumber,
                ExpiryDate = stockBatch.ExpiryDate,
                PurchasePrice = stockBatch.PurchasePrice,
                ReceivedQuantity = stockBatch.ReceivedQuantity,
                AvailableQuantity = stockBatch.AvailableQuantity,
                SupplierId = stockBatch.SupplierId,
                SupplierName = supplier.Name,
                BranchId = stockBatch.BranchId
            };
        }
    }
}
