using MediatR;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Common.Interfaces;
using Pharmacy.Application.DTOs.Purchases;
using Pharmacy.Domain.Entities.Catalog;
using Pharmacy.Domain.Entities.Inventory;
using Pharmacy.Domain.Entities.Partners;
using Pharmacy.Domain.Entities.Purchases;
using Pharmacy.Domain.Enums;
using Pharmacy.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Purchases.Commands.CreatePurchaseReturn
{
    public class CreatePurchaseReturnCommandHandler : IRequestHandler<CreatePurchaseReturnCommand, PurchaseReturnDetailsDto>
    {
        private readonly IRepository<PurchaseReturn> _purchaseReturnRepository;
        private readonly IRepository<PurchaseReturnItem> _purchaseReturnItemRepository;
        private readonly IRepository<PurchaseInvoice> _purchaseInvoiceRepository;
        private readonly IRepository<StockBatch> _stockBatchRepository;
        private readonly IRepository<InventoryTransaction> _inventoryTransactionRepository;
        private readonly IRepository<Supplier> _supplierRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public CreatePurchaseReturnCommandHandler(
            IRepository<PurchaseReturn> purchaseReturnRepository,
            IRepository<PurchaseReturnItem> purchaseReturnItemRepository,
            IRepository<PurchaseInvoice> purchaseInvoiceRepository,
            IRepository<StockBatch> stockBatchRepository,
            IRepository<InventoryTransaction> inventoryTransactionRepository,
            IRepository<Supplier> supplierRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _purchaseReturnRepository = purchaseReturnRepository;
            _purchaseReturnItemRepository = purchaseReturnItemRepository;
            _purchaseInvoiceRepository = purchaseInvoiceRepository;
            _stockBatchRepository = stockBatchRepository;
            _inventoryTransactionRepository = inventoryTransactionRepository;
            _supplierRepository = supplierRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<PurchaseReturnDetailsDto> Handle(CreatePurchaseReturnCommand request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (_currentUserService.BranchId is null)
                throw new UnauthorizedException("لا يوجد فرع مرتبط بالمستخدم الحالي");

            if (request.PurchaseInvoiceId == Guid.Empty)
                throw new BadRequestException("فاتورة الشراء مطلوبة");

            if (string.IsNullOrWhiteSpace(request.Reason))
                throw new BadRequestException("سبب المرتجع مطلوب");

            if (request.Items is null || request.Items.Count == 0)
                throw new BadRequestException("يجب إضافة عنصر واحد على الأقل للمرتجع");

            var invoice = await _purchaseInvoiceRepository
                .GetAll()
                .Include(pi => pi.Supplier)
                .FirstOrDefaultAsync(
                    pi => pi.Id == request.PurchaseInvoiceId &&
                          !pi.IsDeleted &&
                          pi.BranchId == _currentUserService.BranchId.Value,
                    cancellationToken);

            if (invoice is null)
                throw new NotFoundException("PurchaseInvoice", request.PurchaseInvoiceId);

            var supplier = await _supplierRepository
                .GetAll()
                .FirstOrDefaultAsync(
                    s => s.Id == invoice.SupplierId &&
                         !s.IsDeleted &&
                         s.BranchId == _currentUserService.BranchId.Value,
                    cancellationToken);

            if (supplier is null)
                throw new NotFoundException("Supplier", invoice.SupplierId);

            foreach (var item in request.Items)
            {
                if (item.StockBatchId == Guid.Empty)
                    throw new BadRequestException("دفعة المخزون مطلوبة لكل عنصر");

                if (item.Quantity <= 0)
                    throw new BadRequestException("الكمية المرتجعة يجب أن تكون أكبر من صفر");
            }

            var batchIds = request.Items
                .Select(i => i.StockBatchId)
                .Distinct()
                .ToList();

            if (batchIds.Count != request.Items.Count)
                throw new BadRequestException("لا يمكن تكرار نفس دفعة المخزون داخل نفس المرتجع");

            var stockBatches = await _stockBatchRepository
                .GetAll()
                .Where(sb => !sb.IsDeleted &&
                             sb.BranchId == _currentUserService.BranchId.Value &&
                             batchIds.Contains(sb.Id))
                .ToListAsync(cancellationToken);

            if (stockBatches.Count != batchIds.Count)
            {
                var existingIds = stockBatches.Select(sb => sb.Id).ToHashSet();
                var missingBatchId = batchIds.First(id => !existingIds.Contains(id));
                throw new NotFoundException("StockBatch", missingBatchId);
            }

            decimal refundAmount = 0;

            foreach (var item in request.Items)
            {
                var stockBatch = stockBatches.First(sb => sb.Id == item.StockBatchId);

                if (stockBatch.SupplierId != invoice.SupplierId)
                    throw new BadRequestException("دفعة المخزون لا تتبع نفس مورد فاتورة الشراء");

                if (stockBatch.AvailableQuantity < item.Quantity)
                    throw new BadRequestException("الكمية المرتجعة أكبر من الكمية المتاحة في الدفعة");

                refundAmount += item.Quantity * stockBatch.PurchasePrice;
            }

            var purchaseReturn = new PurchaseReturn
            {
                Id = Guid.NewGuid(),
                PurchaseInvoiceId = invoice.Id,
                UserId = _currentUserService.UserId.Value,
                RefundAmount = refundAmount,
                Reason = request.Reason.Trim(),
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = _currentUserService.UserId.Value,
                IsDeleted = false
            };

            _purchaseReturnRepository.Add(purchaseReturn);

            foreach (var item in request.Items)
            {
                var stockBatch = stockBatches.First(sb => sb.Id == item.StockBatchId);

                var purchaseReturnItem = new PurchaseReturnItem
                {
                    Id = Guid.NewGuid(),
                    PurchaseReturnId = purchaseReturn.Id,
                    ProductId = stockBatch.ProductId,
                    //StockBatchId = stockBatch.Id,
                    Quantity = item.Quantity,
                    UnitPrice = stockBatch.PurchasePrice,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = _currentUserService.UserId.Value,
                    IsDeleted = false
                };

                _purchaseReturnItemRepository.Add(purchaseReturnItem);

                stockBatch.AvailableQuantity -= item.Quantity;
                stockBatch.UpdatedAt = DateTime.UtcNow;
                stockBatch.UpdatedByUserId = _currentUserService.UserId.Value;
                _stockBatchRepository.Update(stockBatch);

                var inventoryTransaction = new InventoryTransaction
                {
                    Id = Guid.NewGuid(),
                    StockBatchId = stockBatch.Id,
                    Type = TransactionType.PurchaseReturnOut,
                    Quantity = item.Quantity,
                    Reason = $"Purchase return for invoice {invoice.InvoiceNumber}",
                    ReferenceId = purchaseReturn.Id,
                    ReferenceType = ReferenceType.PurchaseReturn,
                    UserId = _currentUserService.UserId.Value,
                    BranchId = _currentUserService.BranchId.Value,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = _currentUserService.UserId.Value,
                    IsDeleted = false
                };

                _inventoryTransactionRepository.Add(inventoryTransaction);
            }

            supplier.PayableAmount -= refundAmount;
            if (supplier.PayableAmount < 0)
                supplier.PayableAmount = 0;

            supplier.UpdatedAt = DateTime.UtcNow;
            supplier.UpdatedByUserId = _currentUserService.UserId.Value;
            _supplierRepository.Update(supplier);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new PurchaseReturnDetailsDto
            {
                PurchaseReturnId = purchaseReturn.Id,
                PurchaseInvoiceId = purchaseReturn.PurchaseInvoiceId,
                InvoiceNumber = invoice.InvoiceNumber,
                UserId = purchaseReturn.UserId,
                RefundAmount = purchaseReturn.RefundAmount,
                Reason = purchaseReturn.Reason,
                CreatedAt = purchaseReturn.CreatedAt,
                BranchId = invoice.BranchId
            };
        }
    }
}
