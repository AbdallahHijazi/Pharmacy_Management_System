using MediatR;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Common.Interfaces;
using Pharmacy.Application.DTOs.Inventory;
using Pharmacy.Domain.Entities.Catalog;
using Pharmacy.Domain.Entities.Inventory;
using Pharmacy.Domain.Entities.Partners;
using Pharmacy.Domain.Enums;
using Pharmacy.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Inventory.Commands.UpdateStockBatch
{
    public class UpdateStockBatchCommandHandler : IRequestHandler<UpdateStockBatchCommand, StockBatchDetailsDto>
    {
        private readonly IRepository<StockBatch> _stockBatchRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<Supplier> _supplierRepository;
        private readonly IRepository<InventoryTransaction> _inventoryTransactionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public UpdateStockBatchCommandHandler(
            IRepository<StockBatch> stockBatchRepository,
            IRepository<Product> productRepository,
            IRepository<Supplier> supplierRepository,
            IRepository<InventoryTransaction> inventoryTransactionRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _stockBatchRepository = stockBatchRepository;
            _productRepository = productRepository;
            _supplierRepository = supplierRepository;
            _inventoryTransactionRepository = inventoryTransactionRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<StockBatchDetailsDto> Handle(UpdateStockBatchCommand request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (_currentUserService.BranchId is null)
                throw new UnauthorizedException("لا يوجد فرع مرتبط بالمستخدم الحالي");

            if (request.ProductId == Guid.Empty)
                throw new BadRequestException("المنتج مطلوب");

            if (request.SupplierId == Guid.Empty)
                throw new BadRequestException("المورد مطلوب");

            if (string.IsNullOrWhiteSpace(request.LotNumber))
                throw new BadRequestException("رقم التشغيلة مطلوب");

            if (request.PurchasePrice < 0)
                throw new BadRequestException("سعر الشراء يجب أن يكون أكبر من أو يساوي صفر");

            if (request.ReceivedQuantity < 0)
                throw new BadRequestException("الكمية المستلمة يجب أن تكون أكبر من أو تساوي صفر");

            var stockBatch = await _stockBatchRepository
                .GetAll()
                .FirstOrDefaultAsync(
                    sb => sb.Id == request.StockBatchId &&
                          !sb.IsDeleted &&
                          sb.BranchId == _currentUserService.BranchId.Value,
                    cancellationToken);

            if (stockBatch is null)
                throw new NotFoundException("StockBatch", request.StockBatchId);

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

            var normalizedBatchNumber = request.LotNumber.Trim();

            var exists = await _stockBatchRepository
                .GetAll()
                .AnyAsync(
                    sb => sb.Id != request.StockBatchId &&
                          !sb.IsDeleted &&
                          sb.ProductId == request.ProductId &&
                          sb.BatchNumber.ToLower() == normalizedBatchNumber.ToLower(),
                    cancellationToken);

            if (exists)
                throw new StatusAlreadyExistsException(request.LotNumber);

            var deltaReceived = request.ReceivedQuantity - stockBatch.ReceivedQuantity;
            var newAvailable = stockBatch.AvailableQuantity + deltaReceived;

            if (newAvailable < 0)
                throw new BadRequestException(
                    "لا يمكن تعديل الكمية المستلمة بهذا الشكل لأن الكمية المتاحة ستصبح سالبة.");

            stockBatch.ProductId = request.ProductId;
            stockBatch.BatchNumber = normalizedBatchNumber;
            stockBatch.ExpiryDate = request.ExpiryDate;
            stockBatch.PurchasePrice = request.PurchasePrice;
            stockBatch.ReceivedQuantity = request.ReceivedQuantity;
            stockBatch.AvailableQuantity = newAvailable;
            stockBatch.SupplierId = request.SupplierId;
            stockBatch.UpdatedAt = DateTime.UtcNow;
            stockBatch.UpdatedByUserId = _currentUserService.UserId.Value;

            _stockBatchRepository.Update(stockBatch);

            if (deltaReceived != 0)
            {
                var abs = Math.Abs(deltaReceived);
                var type = deltaReceived > 0 ? TransactionType.AdjustmentIn : TransactionType.AdjustmentOut;
                var reason = deltaReceived > 0
                    ? $"تصحيح زيادة الكمية المستلمة للدفعة (+{abs}) — التشغيلة {normalizedBatchNumber}"
                    : $"تصحيح نقصان الكمية المستلمة للدفعة (-{abs}) — التشغيلة {normalizedBatchNumber}";

                _inventoryTransactionRepository.Add(new InventoryTransaction
                {
                    Id = Guid.NewGuid(),
                    StockBatchId = stockBatch.Id,
                    Type = type,
                    Quantity = abs,
                    Reason = reason,
                    ReferenceId = stockBatch.Id,
                    ReferenceType = ReferenceType.StockBatchAdjustment,
                    UserId = _currentUserService.UserId.Value,
                    BranchId = _currentUserService.BranchId.Value,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = _currentUserService.UserId.Value,
                    IsDeleted = false
                });
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new StockBatchDetailsDto
            {
                StockBatchId = stockBatch.Id,
                ProductId = stockBatch.ProductId,
                ProductName = product.Name,
                LotNumber = stockBatch.BatchNumber,
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
