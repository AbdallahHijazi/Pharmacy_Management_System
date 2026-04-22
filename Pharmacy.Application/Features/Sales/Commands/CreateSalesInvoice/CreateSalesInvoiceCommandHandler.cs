using MediatR;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Common.Interfaces;
using Pharmacy.Application.DTOs.Sales;
using Pharmacy.Domain.Entities.Catalog;
using Pharmacy.Domain.Entities.Inventory;
using Pharmacy.Domain.Entities.Sales;
using Pharmacy.Domain.Enums;
using Pharmacy.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Sales.Commands.CreateSalesInvoice
{
    public class CreateSalesInvoiceCommandHandler : IRequestHandler<CreateSalesInvoiceCommand, SalesInvoiceDetailsDto>
    {
        private readonly IRepository<SalesInvoice> _salesInvoiceRepository;
        private readonly IRepository<SalesInvoiceItem> _salesInvoiceItemRepository;
        private readonly IRepository<StockBatch> _stockBatchRepository;
        private readonly IRepository<InventoryTransaction> _inventoryTransactionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public CreateSalesInvoiceCommandHandler(
            IRepository<SalesInvoice> salesInvoiceRepository,
            IRepository<SalesInvoiceItem> salesInvoiceItemRepository,
            IRepository<StockBatch> stockBatchRepository,
            IRepository<InventoryTransaction> inventoryTransactionRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _salesInvoiceRepository = salesInvoiceRepository;
            _salesInvoiceItemRepository = salesInvoiceItemRepository;
            _stockBatchRepository = stockBatchRepository;
            _inventoryTransactionRepository = inventoryTransactionRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<SalesInvoiceDetailsDto> Handle(CreateSalesInvoiceCommand request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null || _currentUserService.BranchId is null)
                throw new UnauthorizedException("المستخدم غير مصرح");

            if (request.Items == null || request.Items.Count == 0)
                throw new BadRequestException("يجب إضافة عناصر");

            if (!Enum.TryParse<PaymentMethod>(request.PaymentMethod, true, out var paymentMethod))
                throw new BadRequestException("طريقة الدفع غير صحيحة");

            decimal subtotal = 0;

            var salesInvoice = new SalesInvoice
            {
                Id = Guid.NewGuid(),
                InvoiceNumber = $"S-{DateTime.UtcNow.Ticks}",
                CustomerId = request.CustomerId,
                UserId = _currentUserService.UserId.Value,
                BranchId = _currentUserService.BranchId.Value,
                DiscountPercentage = request.DiscountPercentage,
                PaymentMethod = paymentMethod,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = _currentUserService.UserId.Value,
                IsDeleted = false
            };

            _salesInvoiceRepository.Add(salesInvoice);

            foreach (var item in request.Items)
            {
                var batches = await _stockBatchRepository
                    .GetAll()
                    .Where(sb => sb.ProductId == item.ProductId &&
                                 sb.BranchId == _currentUserService.BranchId.Value &&
                                 sb.AvailableQuantity > 0 &&
                                 !sb.IsDeleted)
                    .OrderBy(sb => sb.ExpiryDate) // FEFO
                    .ToListAsync(cancellationToken);

                int remainingQty = item.Quantity;

                foreach (var batch in batches)
                {
                    if (remainingQty <= 0) break;

                    int taken = Math.Min(batch.AvailableQuantity, remainingQty);

                    batch.AvailableQuantity -= taken;
                    _stockBatchRepository.Update(batch);

                    var invoiceItem = new SalesInvoiceItem
                    {
                        Id = Guid.NewGuid(),
                        SalesInvoiceId = salesInvoice.Id,
                        StockBatchId = batch.Id,
                        Quantity = taken,
                        UnitPrice = batch.PurchasePrice, // أو سعر البيع إذا عندك
                        Subtotal = taken * batch.PurchasePrice,
                        CreatedAt = DateTime.UtcNow,
                        CreatedByUserId = _currentUserService.UserId.Value,
                        IsDeleted = false
                    };

                    _salesInvoiceItemRepository.Add(invoiceItem);

                    subtotal += invoiceItem.Subtotal;

                    var transaction = new InventoryTransaction
                    {
                        Id = Guid.NewGuid(),
                        StockBatchId = batch.Id,
                        Type = TransactionType.SaleOut,
                        Quantity = taken,
                        ReferenceId = salesInvoice.Id,
                        ReferenceType = ReferenceType.SalesInvoice,
                        UserId = _currentUserService.UserId.Value,
                        BranchId = _currentUserService.BranchId.Value,
                        CreatedAt = DateTime.UtcNow,
                        CreatedByUserId = _currentUserService.UserId.Value,
                        IsDeleted = false,
                        Reason = "POS Sale"
                    };

                    _inventoryTransactionRepository.Add(transaction);

                    remainingQty -= taken;
                }

                if (remainingQty > 0)
                    throw new BadRequestException("المخزون غير كافي");
            }

            var discountAmount = subtotal * (request.DiscountPercentage / 100);
            var grandTotal = subtotal - discountAmount;

            var remainingAmount = grandTotal - request.PaidAmount;

            salesInvoice.Subtotal = subtotal;
            salesInvoice.DiscountAmount = discountAmount;
            salesInvoice.GrandTotal = grandTotal;
            salesInvoice.PaidAmount = request.PaidAmount;
            salesInvoice.RemainingAmount = remainingAmount;

            salesInvoice.Status = remainingAmount <= 0
                ? SalesInvoiceStatus.Completed
                : SalesInvoiceStatus.PartiallyPaid;

            _salesInvoiceRepository.Update(salesInvoice);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new SalesInvoiceDetailsDto
            {
                SalesInvoiceId = salesInvoice.Id,
                InvoiceNumber = salesInvoice.InvoiceNumber,
                GrandTotal = salesInvoice.GrandTotal,
                PaidAmount = salesInvoice.PaidAmount,
                RemainingAmount = salesInvoice.RemainingAmount,
                Status = salesInvoice.Status.ToString()
            };
        }
    }
}
