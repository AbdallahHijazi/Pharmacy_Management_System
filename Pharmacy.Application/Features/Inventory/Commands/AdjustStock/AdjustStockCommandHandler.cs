using MediatR;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Common.Accounting;
using Pharmacy.Application.Common.Interfaces;
using Pharmacy.Application.Common.Inventory;
using Pharmacy.Domain.Entities.Catalog;
using Pharmacy.Domain.Entities.Inventory;
using Pharmacy.Domain.Enums;
using Pharmacy.Domain.Exceptions;

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

            if (!StockBatchManualAdjustment.IsAllowed(transactionType))
            {
                throw new BadRequestException(
                    "نوع الحركة غير مدعوم في التعديل اليدوي. استخدم AdjustmentIn أو AdjustmentOut أو ExpiredWriteOff فقط. " +
                    "للشراء أو البيع أو الدفعة اليدوية استخدم المسارات المخصصة.");
            }

            if (!StockBatchManualAdjustment.TryGetQuantityDeltas(
                    transactionType,
                    request.Quantity,
                    out var availableDelta,
                    out var receivedDelta,
                    out var bonusDelta))
            {
                throw new BadRequestException("نوع حركة المخزون غير مدعوم");
            }

            var stockBatch = await _stockBatchRepository
                .GetAll()
                .FirstOrDefaultAsync(
                    sb => sb.Id == request.StockBatchId &&
                          !sb.IsDeleted &&
                          sb.BranchId == _currentUserService.BranchId.Value,
                    cancellationToken);

            if (stockBatch is null)
                throw new NotFoundException("StockBatch", request.StockBatchId);

            StockBatchManualAdjustment.ValidateInvariants(stockBatch);

            if (availableDelta < 0 && stockBatch.AvailableQuantity + availableDelta < 0)
                throw new BadRequestException("لا يمكن أن تصبح الكمية المتاحة أقل من صفر");

            if (StockBatchEffectiveUnitCost.HasInvalidCostBasis(stockBatch))
            {
                throw new BadRequestException(
                    "أساس تكلفة الدفعة غير صالح (كل الوحدات المستلمة بونص). صحّح الدفعة قبل التعديل اليدوي.");
            }

            StockBatchManualAdjustment.Apply(stockBatch, availableDelta, receivedDelta, bonusDelta);

            if (StockBatchEffectiveUnitCost.HasInvalidCostBasis(stockBatch))
            {
                throw new BadRequestException(
                    "التعديل ينتج دفعة بأساس تكلفة غير صالح. راجع كميات البونص والمستلم.");
            }

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
