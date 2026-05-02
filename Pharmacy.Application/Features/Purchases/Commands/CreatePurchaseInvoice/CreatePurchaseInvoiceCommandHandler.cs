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

namespace Pharmacy.Application.Features.Purchases.Commands.CreatePurchaseInvoice
{
    public class CreatePurchaseInvoiceCommandHandler : IRequestHandler<CreatePurchaseInvoiceCommand, PurchaseInvoiceDetailsDto>
    {
        private readonly IRepository<PurchaseInvoice> _purchaseInvoiceRepository;
        private readonly IRepository<PurchaseInvoiceItem> _purchaseInvoiceItemRepository;
        private readonly IRepository<Supplier> _supplierRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<StockBatch> _stockBatchRepository;
        private readonly IRepository<InventoryTransaction> _inventoryTransactionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public CreatePurchaseInvoiceCommandHandler(
            IRepository<PurchaseInvoice> purchaseInvoiceRepository,
            IRepository<PurchaseInvoiceItem> purchaseInvoiceItemRepository,
            IRepository<Supplier> supplierRepository,
            IRepository<Product> productRepository,
            IRepository<StockBatch> stockBatchRepository,
            IRepository<InventoryTransaction> inventoryTransactionRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _purchaseInvoiceRepository = purchaseInvoiceRepository;
            _purchaseInvoiceItemRepository = purchaseInvoiceItemRepository;
            _supplierRepository = supplierRepository;
            _productRepository = productRepository;
            _stockBatchRepository = stockBatchRepository;
            _inventoryTransactionRepository = inventoryTransactionRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<PurchaseInvoiceDetailsDto> Handle(CreatePurchaseInvoiceCommand request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (_currentUserService.BranchId is null)
                throw new UnauthorizedException("لا يوجد فرع مرتبط بالمستخدم الحالي");

            if (request.SupplierId == Guid.Empty)
                throw new BadRequestException("المورد مطلوب");

            if (string.IsNullOrWhiteSpace(request.InvoiceNumber))
                throw new BadRequestException("رقم الفاتورة مطلوب");

            if (string.IsNullOrWhiteSpace(request.PaymentMethod))
                throw new BadRequestException("طريقة الدفع مطلوبة");

            if (request.TaxRate < 0)
                throw new BadRequestException("نسبة الضريبة يجب أن تكون أكبر من أو تساوي صفر");

            if (request.PaidAmount < 0)
                throw new BadRequestException("المبلغ المدفوع يجب أن يكون أكبر من أو يساوي صفر");

            if (request.Items is null || request.Items.Count == 0)
                throw new BadRequestException("يجب إضافة عنصر واحد على الأقل للفاتورة");

            var supplier = await _supplierRepository
                .GetAll()
                .FirstOrDefaultAsync(
                    s => s.Id == request.SupplierId &&
                         !s.IsDeleted &&
                         s.BranchId == _currentUserService.BranchId.Value,
                    cancellationToken);

            if (supplier is null)
                throw new NotFoundException("Supplier", request.SupplierId);

            var normalizedInvoiceNumber = request.InvoiceNumber.Trim();

            var invoiceNumberExists = await _purchaseInvoiceRepository
                .GetAll()
                .AnyAsync(
                    pi => !pi.IsDeleted && pi.InvoiceNumber.ToLower() == normalizedInvoiceNumber.ToLower(),
                    cancellationToken);

            if (invoiceNumberExists)
                throw new StatusAlreadyExistsException(request.InvoiceNumber);

            var productIds = request.Items
                .Select(i => i.ProductId)
                .Distinct()
                .ToList();

            var products = await _productRepository
                .GetAll()
                .Where(p => !p.IsDeleted &&
                            p.BranchId == _currentUserService.BranchId.Value &&
                            productIds.Contains(p.Id))
                .ToListAsync(cancellationToken);

            if (products.Count != productIds.Count)
            {
                var existingIds = products.Select(p => p.Id).ToHashSet();
                var missingProductId = productIds.First(id => !existingIds.Contains(id));
                throw new NotFoundException("Product", missingProductId);
            }

            foreach (var item in request.Items)
            {
                if (item.ProductId == Guid.Empty)
                    throw new BadRequestException("معرف المنتج مطلوب");

                if (string.IsNullOrWhiteSpace(item.BatchNumber))
                    throw new BadRequestException("رقم التشغيلة مطلوب لكل عنصر");

                if (item.Quantity <= 0)
                    throw new BadRequestException("الكمية يجب أن تكون أكبر من صفر");

                if (item.UnitPrice < 0)
                    throw new BadRequestException("سعر الوحدة يجب أن يكون أكبر من أو يساوي صفر");
            }

            var duplicatedBatch = request.Items
                .GroupBy(i => new { i.ProductId, BatchNumber = i.BatchNumber.Trim().ToLower() })
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicatedBatch is not null)
                throw new BadRequestException("لا يمكن تكرار نفس التشغيلة لنفس المنتج داخل نفس الفاتورة");

            var requestedBatches = request.Items
                .Select(i => new
                {
                    ProductId = i.ProductId,
                    BatchNumber = i.BatchNumber.Trim().ToLower()
                })
                .ToList();

            var requestedProductIds = requestedBatches
                .Select(x => x.ProductId)
                .Distinct()
                .ToList();

            var existingBatches = await _stockBatchRepository
                .GetAll()
                .Where(sb =>
                    !sb.IsDeleted &&
                    sb.BranchId == _currentUserService.BranchId.Value &&
                    requestedProductIds.Contains(sb.ProductId))
                .Select(sb => new
                {
                    sb.ProductId,
                    sb.BatchNumber
                })
                .ToListAsync(cancellationToken);

            var existingBatchConflict = existingBatches.Any(sb =>
                requestedBatches.Any(rb =>
                    rb.ProductId == sb.ProductId &&
                    rb.BatchNumber == sb.BatchNumber.Trim().ToLower()));

            if (existingBatchConflict)
                throw new BadRequestException("إحدى التشغيلات موجودة مسبقاً لهذا المنتج");

            var subtotal = request.Items.Sum(i => i.Quantity * i.UnitPrice);
            var taxAmount = subtotal * (request.TaxRate / 100m);
            var grandTotal = subtotal + taxAmount;

            if (request.PaidAmount > grandTotal)
                throw new BadRequestException("المبلغ المدفوع لا يمكن أن يكون أكبر من إجمالي الفاتورة");

            var remainingAmount = grandTotal - request.PaidAmount;
            if (!Enum.TryParse<PaymentMethod>(request.PaymentMethod, true, out var paymentMethod))
                throw new BadRequestException("طريقة الدفع غير صحيحة");

            //var status = remainingAmount == 0
            //    ? PurchaseInvoiceStatus.Completed
            //    : (request.PaidAmount > 0
            //        ? PurchaseInvoiceStatus.PartiallyPaid
            //        : PurchaseInvoiceStatus.Pending);

            PurchaseInvoiceStatus status;

            if (remainingAmount == 0)
            {
                status = PurchaseInvoiceStatus.Completed;
            }
            else if (request.PaidAmount > 0)
            {
                status = PurchaseInvoiceStatus.PartiallyPaid;
            }
            else
            {
                status = PurchaseInvoiceStatus.Pending;
            }

            var invoice = new PurchaseInvoice
            {
                Id = Guid.NewGuid(),
                InvoiceNumber = normalizedInvoiceNumber,
                SupplierId = request.SupplierId,
                UserId = _currentUserService.UserId.Value,
                BranchId = _currentUserService.BranchId.Value,
                Subtotal = subtotal,
                TaxRate = request.TaxRate,
                TaxAmount = taxAmount,
                GrandTotal = grandTotal,
                PaidAmount = request.PaidAmount,
                RemainingAmount = remainingAmount,
                PaymentMethod = paymentMethod,
                Status = status,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = _currentUserService.UserId.Value,
                IsDeleted = false
            };

            _purchaseInvoiceRepository.Add(invoice);

            foreach (var item in request.Items)
            {
                var normalizedBatchNumber = item.BatchNumber.Trim();

                var invoiceItem = new PurchaseInvoiceItem
                {
                    Id = Guid.NewGuid(),
                    PurchaseInvoiceId = invoice.Id,
                    ProductId = item.ProductId,
                    BatchNumber = normalizedBatchNumber,
                    ExpiryDate = item.ExpiryDate,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = _currentUserService.UserId.Value,
                    IsDeleted = false
                };

                _purchaseInvoiceItemRepository.Add(invoiceItem);

                var stockBatch = new StockBatch
                {
                    Id = Guid.NewGuid(),
                    ProductId = item.ProductId,
                    BatchNumber = normalizedBatchNumber,
                    ExpiryDate = item.ExpiryDate,
                    PurchasePrice = item.UnitPrice,
                    ReceivedQuantity = item.Quantity,
                    AvailableQuantity = item.Quantity,
                    SupplierId = request.SupplierId,
                    BranchId = _currentUserService.BranchId.Value,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = _currentUserService.UserId.Value,
                    IsDeleted = false
                };

                _stockBatchRepository.Add(stockBatch);

                var inventoryTransaction = new InventoryTransaction
                {
                    Id = Guid.NewGuid(),
                    StockBatchId = stockBatch.Id,
                    Type = TransactionType.PurchaseIn,
                    Quantity = item.Quantity,
                    Reason = $"Purchase invoice {normalizedInvoiceNumber}",
                    ReferenceId = invoice.Id,
                    ReferenceType = ReferenceType.PurchaseInvoice,
                    UserId = _currentUserService.UserId.Value,
                    BranchId = _currentUserService.BranchId.Value,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = _currentUserService.UserId.Value,
                    IsDeleted = false
                };

                _inventoryTransactionRepository.Add(inventoryTransaction);
            }

            supplier.TotalPurchases += grandTotal;
            supplier.PayableAmount += remainingAmount;
            supplier.UpdatedAt = DateTime.UtcNow;
            supplier.UpdatedByUserId = _currentUserService.UserId.Value;

            _supplierRepository.Update(supplier);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new PurchaseInvoiceDetailsDto
            {
                PurchaseInvoiceId = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                SupplierId = invoice.SupplierId,
                SupplierName = supplier.Name,
                UserId = invoice.UserId,
                BranchId = invoice.BranchId,
                Subtotal = invoice.Subtotal,
                TaxRate = invoice.TaxRate,
                TaxAmount = invoice.TaxAmount,
                GrandTotal = invoice.GrandTotal,
                PaidAmount = invoice.PaidAmount,
                RemainingAmount = invoice.RemainingAmount,
                PaymentMethod = invoice.PaymentMethod.ToString(),
                Status = invoice.Status.ToString(),
                CreatedAt = invoice.CreatedAt
            };
        }
    }
}
