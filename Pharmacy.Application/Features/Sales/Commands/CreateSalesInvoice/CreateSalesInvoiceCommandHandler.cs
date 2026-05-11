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
        private readonly IStockBatchConcurrencyRetryPolicy _stockBatchConcurrencyRetry;

        public CreateSalesInvoiceCommandHandler(
            IRepository<SalesInvoice> salesInvoiceRepository,
            IRepository<SalesInvoiceItem> salesInvoiceItemRepository,
            IRepository<StockBatch> stockBatchRepository,
            IRepository<InventoryTransaction> inventoryTransactionRepository,
            IRepository<Product> productRepository,
            IRepository<Customer> customerRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IStockBatchConcurrencyRetryPolicy stockBatchConcurrencyRetry)
        {
            _salesInvoiceRepository = salesInvoiceRepository;
            _salesInvoiceItemRepository = salesInvoiceItemRepository;
            _stockBatchRepository = stockBatchRepository;
            _inventoryTransactionRepository = inventoryTransactionRepository;
            _productRepository = productRepository;
            _customerRepository = customerRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _stockBatchConcurrencyRetry = stockBatchConcurrencyRetry;
        }

        public async Task<SalesInvoiceDetailsDto> Handle(CreateSalesInvoiceCommand request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null || _currentUserService.BranchId is null)
                throw new UnauthorizedException("المستخدم غير مصرح");

            var userId = _currentUserService.UserId.Value;
            var branchId = _currentUserService.BranchId.Value;

            var salesInvoiceId = Guid.NewGuid();
            var invoiceNumber = $"S-{DateTime.UtcNow.Ticks}";

            var prepared = await PrepareCreateSalesInvoiceAsync(
                request,
                userId,
                branchId,
                salesInvoiceId,
                invoiceNumber,
                cancellationToken);

            var result = await _stockBatchConcurrencyRetry.ExecuteAsync(
                () => CommitSalesInvoiceWithStockDeductionAsync(prepared, cancellationToken),
                cancellationToken);

            // Concurrency retry wraps only CommitSalesInvoiceWithStockDeductionAsync.
            // Add printing, notifications, messaging, loyalty, or any external side effects here after a successful commit — never inside the retry delegate.

            return result;
        }

        /// <summary>
        /// Read-only validation and pricing inputs. Must stay outside optimistic-concurrency retry.
        /// </summary>
        private async Task<PreparedSalesInvoice> PrepareCreateSalesInvoiceAsync(
            CreateSalesInvoiceCommand request,
            Guid userId,
            Guid branchId,
            Guid salesInvoiceId,
            string invoiceNumber,
            CancellationToken cancellationToken)
        {
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

            var productsById = products.ToDictionary(p => p.Id);

            decimal subtotal = 0;
            foreach (var item in request.Items)
                subtotal += item.Quantity * productsById[item.ProductId].SellingPrice;

            var discountAmount = subtotal * (request.DiscountPercentage / 100m);
            var grandTotal = subtotal - discountAmount;

            if (request.PaidAmount > grandTotal)
                throw new BadRequestException("المبلغ المدفوع لا يمكن أن يكون أكبر من إجمالي الفاتورة");

            var remainingAmount = grandTotal - request.PaidAmount;

            var status = remainingAmount <= 0
                ? SalesInvoiceStatus.Completed
                : SalesInvoiceStatus.PartiallyPaid;

            return new PreparedSalesInvoice(
                request,
                userId,
                branchId,
                salesInvoiceId,
                invoiceNumber,
                productsById,
                paymentMethod,
                subtotal,
                discountAmount,
                grandTotal,
                remainingAmount,
                status);
        }

        /// <summary>
        /// Stock batch reads/updates, invoice persistence. This is the only code retried on <see cref="Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException"/>.
        /// </summary>
        private async Task<SalesInvoiceDetailsDto> CommitSalesInvoiceWithStockDeductionAsync(
            PreparedSalesInvoice prepared,
            CancellationToken cancellationToken)
        {
            var request = prepared.Request;
            var userId = prepared.UserId;
            var branchId = prepared.BranchId;
            var salesInvoiceId = prepared.SalesInvoiceId;
            var invoiceNumber = prepared.InvoiceNumber;

            var invoiceItems = new List<SalesInvoiceItem>();
            var inventoryTransactions = new List<InventoryTransaction>();

            var productIdsForStock = request.Items
                .Select(i => i.ProductId)
                .Distinct()
                .ToList();

            var allEligibleBatches = await _stockBatchRepository
                .GetAll()
                .Where(sb =>
                    productIdsForStock.Contains(sb.ProductId) &&
                    sb.BranchId == branchId &&
                    sb.AvailableQuantity > 0 &&
                    !sb.IsDeleted)
                .ToListAsync(cancellationToken);

            var batchesByProductId = allEligibleBatches
                .GroupBy(sb => sb.ProductId)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(sb => sb.ExpiryDate).ToList());

            foreach (var item in request.Items)
            {
                var product = prepared.ProductsById[item.ProductId];

                var batches = batchesByProductId.TryGetValue(item.ProductId, out var batchList)
                    ? batchList
                    : [];

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

            var salesInvoice = new SalesInvoice
            {
                Id = salesInvoiceId,
                InvoiceNumber = invoiceNumber,
                CustomerId = request.CustomerId,
                UserId = userId,
                BranchId = branchId,
                Subtotal = prepared.Subtotal,
                DiscountPercentage = request.DiscountPercentage,
                DiscountAmount = prepared.DiscountAmount,
                TaxRate = 0,
                TaxAmount = 0,
                GrandTotal = prepared.GrandTotal,
                PaidAmount = request.PaidAmount,
                RemainingAmount = prepared.RemainingAmount,
                PaymentMethod = prepared.PaymentMethod,
                Status = prepared.Status,
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

        private sealed record PreparedSalesInvoice(
            CreateSalesInvoiceCommand Request,
            Guid UserId,
            Guid BranchId,
            Guid SalesInvoiceId,
            string InvoiceNumber,
            Dictionary<Guid, Product> ProductsById,
            PaymentMethod PaymentMethod,
            decimal Subtotal,
            decimal DiscountAmount,
            decimal GrandTotal,
            decimal RemainingAmount,
            SalesInvoiceStatus Status);
    }
}
