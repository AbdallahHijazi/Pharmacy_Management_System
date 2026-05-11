using MediatR;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Common.Interfaces;
using Pharmacy.Domain.Entities.Catalog;
using Pharmacy.Domain.Entities.Inventory;
using Pharmacy.Domain.Enums;
using Pharmacy.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Inventory.Commands.AdjustStock
{
    public class AdjustStockCommandHandler : IRequestHandler<AdjustStockCommand, Unit>
    {
        private readonly IRepository<StockBatch> _stockBatchRepository;
        private readonly IRepository<InventoryTransaction> _inventoryTransactionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public AdjustStockCommandHandler(
            IRepository<StockBatch> stockBatchRepository,
            IRepository<InventoryTransaction> inventoryTransactionRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _stockBatchRepository = stockBatchRepository;
            _inventoryTransactionRepository = inventoryTransactionRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Unit> Handle(AdjustStockCommand request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (_currentUserService.BranchId is null)
                throw new UnauthorizedException("لا يوجد فرع مرتبط بالمستخدم الحالي");

            if (request.StockBatchId == Guid.Empty)
                throw new BadRequestException("دفعة المخزون مطلوبة");

            if (request.Quantity <= 0)
                throw new BadRequestException("الكمية يجب أن تكون أكبر من صفر");

            if (string.IsNullOrWhiteSpace(request.Reason))
                throw new BadRequestException("سبب التعديل مطلوب");

            if (!Enum.TryParse<TransactionType>(request.Type, true, out var transactionType))
                throw new BadRequestException("نوع حركة المخزون غير صحيح");

            var stockBatch = await _stockBatchRepository
                .GetAll()
                .FirstOrDefaultAsync(
                    sb => sb.Id == request.StockBatchId &&
                          !sb.IsDeleted &&
                          sb.BranchId == _currentUserService.BranchId.Value,
                    cancellationToken);

            if (stockBatch is null)
                throw new NotFoundException("StockBatch", request.StockBatchId);

            var newAvailableQuantity = stockBatch.AvailableQuantity;

            switch (transactionType)
            {
                case TransactionType.PurchaseIn:
                case TransactionType.ReturnIn:
                case TransactionType.AdjustmentIn:
                    newAvailableQuantity += request.Quantity;
                    break;

                case TransactionType.SaleOut:
                case TransactionType.AdjustmentOut:
                case TransactionType.ExpiredWriteOff:
                    newAvailableQuantity -= request.Quantity;
                    break;

                default:
                    throw new BadRequestException("نوع حركة المخزون غير مدعوم");
            }

            if (newAvailableQuantity < 0)
                throw new BadRequestException("لا يمكن أن تصبح الكمية المتاحة أقل من صفر");

            stockBatch.AvailableQuantity = newAvailableQuantity;
            stockBatch.UpdatedAt = DateTime.UtcNow;
            stockBatch.UpdatedByUserId = _currentUserService.UserId.Value;

            _stockBatchRepository.Update(stockBatch);

            var transaction = new InventoryTransaction
            {
                Id = Guid.NewGuid(),
                StockBatchId = stockBatch.Id,
                Type = transactionType,
                Quantity = request.Quantity,
                Reason = request.Reason.Trim(),
                ReferenceId = null,
                ReferenceType = ReferenceType.StockBatchAdjustment,
                UserId = _currentUserService.UserId.Value,
                BranchId = _currentUserService.BranchId.Value,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = _currentUserService.UserId.Value,
                IsDeleted = false
            };

            _inventoryTransactionRepository.Add(transaction);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
