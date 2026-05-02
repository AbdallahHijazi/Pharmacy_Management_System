using MediatR;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Common.Interfaces;
using Pharmacy.Application.DTOs.Sales;
using Pharmacy.Domain.Entities.Catalog;
using Pharmacy.Domain.Entities.Inventory;
using Pharmacy.Domain.Entities.Partners;
using Pharmacy.Domain.Entities.Sales;
using Pharmacy.Domain.Enums;
using Pharmacy.Domain.Exceptions;

namespace Pharmacy.Application.Features.Sales.Commands.CreateSalesInvoice
{
    public class CreateSalesInvoiceCommandHandler : IRequestHandler<CreateSalesInvoiceCommand, SalesInvoiceDetailsDto>
    {
        private readonly IRepository<SalesInvoice> _salesInvoiceRepository;
        private readonly IRepository<SalesInvoiceItem> _salesInvoiceItemRepository;
        private readonly IRepository<StockBatch> _stockBatchRepository;
        private readonly IRepository<InventoryTransaction> _inventoryTransactionRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public CreateSalesInvoiceCommandHandler(
            IRepository<SalesInvoice> salesInvoiceRepository,
            IRepository<SalesInvoiceItem> salesInvoiceItemRepository,
            IRepository<StockBatch> stockBatchRepository,
            IRepository<InventoryTransaction> inventoryTransactionRepository,
            IRepository<Product> productRepository,
            IRepository<Customer> customerRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _salesInvoiceRepository = salesInvoiceRepository;
            _salesInvoiceItemRepository = salesInvoiceItemRepository;
            _stockBatchRepository = stockBatchRepository;
            _inventoryTransactionRepository = inventoryTransactionRepository;
            _productRepository = productRepository;
            _customerRepository = customerRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<SalesInvoiceDetailsDto> Handle(CreateSalesInvoiceCommand request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null || _currentUserService.BranchId is null)
                throw new UnauthorizedException("المستخدم غير مصرح");

            var userId = _currentUserService.UserId.Value;
            var branchId = _currentUserService.BranchId.Value;

            if (request.Items == null || request.Items.Count == 0)
                throw new BadRequestException("يجب إضافة عناصر");

            if (string.IsNullOrWhiteSpace(request.PaymentMethod))
                throw new BadRequestException("طريقة الدفع مطلوبة");

            if (!Enum.TryParse<PaymentMethod>(request.PaymentMethod, true, out var paymentMethod))
                throw new BadRequestException("طريقة الدفع غير صحيحة");

            if (request.DiscountPercentage < 0 || request.DiscountPercentage > 100)
                throw new BadRequestException("نسبة الخصم غير صحيحة");

            if (request.PaidAmount < 0)
                throw new BadRequestException("المبلغ المدفوع غير صحيح");

            foreach (var item in request.Items)
            {
                if (item.ProductId == Guid.Empty)
                    throw new BadRequestException("معرف المنتج مطلوب");

                if (item.Quantity <= 0)
                    throw new BadRequestException("الكمية يجب أن تكون أكبر من صفر");
            }

            if (request.CustomerId.HasValue)
            {
                var customerExists = await _customerRepository
                    .GetAll()
                    .AnyAsync(c =>
                        c.Id == request.CustomerId.Value &&
                        c.BranchId == branchId &&
                        !c.IsDeleted,
                        cancellationToken);

                if (!customerExists)
                    throw new NotFoundException("Customer", request.CustomerId.Value);
            }

            var productIds = request.Items
                .Select(i => i.ProductId)
                .Distinct()
                .ToList();

            var products = await _productRepository
                .GetAll()
                .Where(p =>
                    productIds.Contains(p.Id) &&
                    p.BranchId == branchId &&
                    !p.IsDeleted)
                .ToListAsync(cancellationToken);

            if (products.Count != productIds.Count)
            {
                var existingProductIds = products.Select(p => p.Id).ToHashSet();
                var missingProductId = productIds.First(id => !existingProductIds.Contains(id));

                throw new NotFoundException("Product", missingProductId);
            }

            decimal subtotal = 0;

            var salesInvoiceId = Guid.NewGuid();
            var invoiceNumber = $"S-{DateTime.UtcNow.Ticks}";

            var invoiceItems = new List<SalesInvoiceItem>();
            var inventoryTransactions = new List<InventoryTransaction>();

            foreach (var item in request.Items)
            {
                var product = products.First(p => p.Id == item.ProductId);

                var batches = await _stockBatchRepository
                    .GetAll()
                    .Where(sb =>
                        sb.ProductId == item.ProductId &&
                        sb.BranchId == branchId &&
                        sb.AvailableQuantity > 0 &&
                        !sb.IsDeleted)
                    .OrderBy(sb => sb.ExpiryDate)
                    .ToListAsync(cancellationToken);

                var totalAvailable = batches.Sum(b => b.AvailableQuantity);

                if (totalAvailable < item.Quantity)
                    throw new BadRequestException("المخزون غير كافي");

                var remainingQty = item.Quantity;

                foreach (var batch in batches)
                {
                    if (remainingQty <= 0)
                        break;

                    var taken = Math.Min(batch.AvailableQuantity, remainingQty);

                    batch.AvailableQuantity -= taken;
                    _stockBatchRepository.Update(batch);

                    var invoiceItem = new SalesInvoiceItem
                    {
                        Id = Guid.NewGuid(),
                        SalesInvoiceId = salesInvoiceId,
                        StockBatchId = batch.Id,
                        Quantity = taken,
                        UnitPrice = product.SellingPrice,
                        Subtotal = taken * product.SellingPrice,
                        CreatedAt = DateTime.UtcNow,
                        CreatedByUserId = userId,
                        IsDeleted = false
                    };

                    invoiceItems.Add(invoiceItem);
                    subtotal += invoiceItem.Subtotal;

                    var transaction = new InventoryTransaction
                    {
                        Id = Guid.NewGuid(),
                        StockBatchId = batch.Id,
                        Type = TransactionType.SaleOut,
                        Quantity = taken,
                        Reason = $"Sales invoice {invoiceNumber}",
                        ReferenceId = salesInvoiceId,
                        ReferenceType = ReferenceType.SalesInvoice,
                        UserId = userId,
                        BranchId = branchId,
                        CreatedAt = DateTime.UtcNow,
                        CreatedByUserId = userId,
                        IsDeleted = false
                    };

                    inventoryTransactions.Add(transaction);

                    remainingQty -= taken;
                }
            }

            var discountAmount = subtotal * (request.DiscountPercentage / 100m);
            var grandTotal = subtotal - discountAmount;

            if (request.PaidAmount > grandTotal)
                throw new BadRequestException("المبلغ المدفوع لا يمكن أن يكون أكبر من إجمالي الفاتورة");

            var remainingAmount = grandTotal - request.PaidAmount;

            var status = remainingAmount <= 0
                ? SalesInvoiceStatus.Completed
                : SalesInvoiceStatus.PartiallyPaid;

            var salesInvoice = new SalesInvoice
            {
                Id = salesInvoiceId,
                InvoiceNumber = invoiceNumber,
                CustomerId = request.CustomerId,
                UserId = userId,
                BranchId = branchId,
                Subtotal = subtotal,
                DiscountPercentage = request.DiscountPercentage,
                DiscountAmount = discountAmount,
                TaxRate = 0,
                TaxAmount = 0,
                GrandTotal = grandTotal,
                PaidAmount = request.PaidAmount,
                RemainingAmount = remainingAmount,
                PaymentMethod = paymentMethod,
                Status = status,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = userId,
                IsDeleted = false
            };

            _salesInvoiceRepository.Add(salesInvoice);

            foreach (var invoiceItem in invoiceItems)
                _salesInvoiceItemRepository.Add(invoiceItem);

            foreach (var transaction in inventoryTransactions)
                _inventoryTransactionRepository.Add(transaction);

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