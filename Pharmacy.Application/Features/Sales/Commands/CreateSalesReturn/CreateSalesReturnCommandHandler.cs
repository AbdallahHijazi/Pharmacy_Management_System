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

namespace Pharmacy.Application.Features.Sales.Commands.CreateSalesReturn
{
    public class CreateSalesReturnCommandHandler : IRequestHandler<CreateSalesReturnCommand, SalesReturnDetailsDto>
    {
        private readonly IRepository<SalesReturn> _salesReturnRepository;
        private readonly IRepository<SalesInvoice> _salesInvoiceRepository;
        private readonly IRepository<SalesInvoiceItem> _salesInvoiceItemRepository;
        private readonly IRepository<StockBatch> _stockBatchRepository;
        private readonly IRepository<InventoryTransaction> _inventoryTransactionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public CreateSalesReturnCommandHandler(
            IRepository<SalesReturn> salesReturnRepository,
            IRepository<SalesInvoice> salesInvoiceRepository,
            IRepository<SalesInvoiceItem> salesInvoiceItemRepository,
            IRepository<StockBatch> stockBatchRepository,
            IRepository<InventoryTransaction> inventoryTransactionRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _salesReturnRepository = salesReturnRepository;
            _salesInvoiceRepository = salesInvoiceRepository;
            _salesInvoiceItemRepository = salesInvoiceItemRepository;
            _stockBatchRepository = stockBatchRepository;
            _inventoryTransactionRepository = inventoryTransactionRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<SalesReturnDetailsDto> Handle(CreateSalesReturnCommand request, CancellationToken cancellationToken)
        {
            // تحقق من صلاحية المستخدم والفرع
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (_currentUserService.BranchId is null)
                throw new UnauthorizedException("لا يوجد فرع مرتبط بالمستخدم");

            if (request.SalesInvoiceId == Guid.Empty)
                throw new BadRequestException("فاتورة البيع مطلوبة");

            if (string.IsNullOrWhiteSpace(request.Reason))
                throw new BadRequestException("سبب المرتجع مطلوب");

            if (request.Items is null || request.Items.Count == 0)
                throw new BadRequestException("يجب إضافة عنصر واحد على الأقل");

            // جلب الفاتورة من قاعدة البيانات
            var invoice = await _salesInvoiceRepository
                .GetAll()
                .FirstOrDefaultAsync(
                    si => si.Id == request.SalesInvoiceId &&
                          !si.IsDeleted &&
                          si.BranchId == _currentUserService.BranchId.Value,
                    cancellationToken);

            if (invoice is null)
                throw new NotFoundException("SalesInvoice", request.SalesInvoiceId);

            // التحقق من صحة العناصر المرتجعة
            foreach (var item in request.Items)
            {
                if (item.SalesInvoiceItemId == Guid.Empty)
                    throw new BadRequestException("عنصر الفاتورة مطلوب");

                if (item.Quantity <= 0)
                    throw new BadRequestException("كمية المرتجع يجب أن تكون أكبر من صفر");
            }

            var invoiceItemIds = request.Items
                .Select(i => i.SalesInvoiceItemId)
                .Distinct()
                .ToList();

            if (invoiceItemIds.Count != request.Items.Count)
                throw new BadRequestException("لا يمكن تكرار نفس عنصر الفاتورة داخل نفس المرتجع");

            var salesInvoiceItems = await _salesInvoiceItemRepository
                .GetAll()
                .Where(sii => !sii.IsDeleted &&
                              sii.SalesInvoiceId == request.SalesInvoiceId &&
                              invoiceItemIds.Contains(sii.Id))
                .ToListAsync(cancellationToken);

            if (salesInvoiceItems.Count != invoiceItemIds.Count)
            {
                var existingIds = salesInvoiceItems.Select(x => x.Id).ToHashSet();
                var missingId = invoiceItemIds.First(id => !existingIds.Contains(id));
                throw new NotFoundException("SalesInvoiceItem", missingId);
            }

            var stockBatchIds = salesInvoiceItems
                .Select(x => x.StockBatchId)
                .Distinct()
                .ToList();

            var stockBatches = await _stockBatchRepository
                .GetAll()
                .Where(sb => !sb.IsDeleted &&
                             sb.BranchId == _currentUserService.BranchId.Value &&
                             stockBatchIds.Contains(sb.Id))
                .ToListAsync(cancellationToken);

            decimal refundAmount = 0m;

            // إنشاء SalesReturn مع تهيئة المجموعة
            var salesReturn = new SalesReturn
            {
                Id = Guid.NewGuid(),
                SalesInvoiceId = invoice.Id,
                UserId = _currentUserService.UserId.Value,
                RefundAmount = 0,
                Reason = request.Reason.Trim(),
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = _currentUserService.UserId.Value,
                IsDeleted = false,
                Items = new List<SalesReturnItem>()
            };

            foreach (var item in request.Items)
            {
                var invoiceItem = salesInvoiceItems.First(x => x.Id == item.SalesInvoiceItemId);
                var stockBatch = stockBatches.First(sb => sb.Id == invoiceItem.StockBatchId);

                if (item.Quantity > invoiceItem.Quantity)
                    throw new BadRequestException("كمية المرتجع لا يمكن أن تكون أكبر من كمية عنصر الفاتورة");

                var lineRefund = item.Quantity * invoiceItem.UnitPrice;
                refundAmount += lineRefund;

                var salesReturnItem = new SalesReturnItem
                {
                    Id = Guid.NewGuid(),
                    SalesReturnId = salesReturn.Id,
                    SalesInvoiceItemId = invoiceItem.Id,
                    Quantity = item.Quantity,
                    UnitPrice = invoiceItem.UnitPrice,
                    StockBatchId = invoiceItem.StockBatchId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = _currentUserService.UserId.Value,
                    IsDeleted = false
                };

                // ✅ الإضافة عبر Navigation Property بدلاً من Repository مباشرةً
                salesReturn.Items.Add(salesReturnItem);

                // زيادة الكمية في المخزون
                stockBatch.AvailableQuantity += item.Quantity;
                stockBatch.UpdatedAt = DateTime.UtcNow;
                stockBatch.UpdatedByUserId = _currentUserService.UserId.Value;
                _stockBatchRepository.Update(stockBatch);

                // إضافة المعاملة للمخزون
                var transaction = new InventoryTransaction
                {
                    Id = Guid.NewGuid(),
                    StockBatchId = stockBatch.Id,
                    Type = TransactionType.ReturnIn,
                    Quantity = item.Quantity,
                    Reason = $"Sales return for invoice {invoice.InvoiceNumber}",
                    ReferenceId = salesReturn.Id,
                    ReferenceType = ReferenceType.SalesReturn,
                    UserId = _currentUserService.UserId.Value,
                    BranchId = _currentUserService.BranchId.Value,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = _currentUserService.UserId.Value,
                    IsDeleted = false
                };

                _inventoryTransactionRepository.Add(transaction);
            }

            salesReturn.RefundAmount = refundAmount;

            // ✅ إضافة SalesReturn بعد تجهيز كل العناصر
            _salesReturnRepository.Add(salesReturn);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new SalesReturnDetailsDto
            {
                SalesReturnId = salesReturn.Id,
                SalesInvoiceId = salesReturn.SalesInvoiceId,
                InvoiceNumber = invoice.InvoiceNumber,
                UserId = salesReturn.UserId,
                RefundAmount = salesReturn.RefundAmount,
                Reason = salesReturn.Reason,
                CreatedAt = salesReturn.CreatedAt,
                BranchId = invoice.BranchId
            };
        }
    }
}
