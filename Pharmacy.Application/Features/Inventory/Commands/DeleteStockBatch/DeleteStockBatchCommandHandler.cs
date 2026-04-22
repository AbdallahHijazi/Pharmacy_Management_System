using MediatR;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Common.Interfaces;
using Pharmacy.Domain.Entities.Catalog;
using Pharmacy.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Inventory.Commands.DeleteStockBatch
{
    public class DeleteStockBatchCommandHandler : IRequestHandler<DeleteStockBatchCommand, Unit>
    {
        private readonly IRepository<StockBatch> _stockBatchRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public DeleteStockBatchCommandHandler(
            IRepository<StockBatch> stockBatchRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _stockBatchRepository = stockBatchRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Unit> Handle(DeleteStockBatchCommand request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (_currentUserService.BranchId is null)
                throw new UnauthorizedException("لا يوجد فرع مرتبط بالمستخدم الحالي");

            var stockBatch = await _stockBatchRepository
                .GetAll()
                .FirstOrDefaultAsync(
                    sb => sb.Id == request.StockBatchId &&
                          !sb.IsDeleted &&
                          sb.BranchId == _currentUserService.BranchId.Value,
                    cancellationToken);

            if (stockBatch is null)
                throw new NotFoundException("StockBatch", request.StockBatchId);

            stockBatch.IsDeleted = true;
            stockBatch.DeletedAt = DateTime.UtcNow;
            stockBatch.UpdatedAt = DateTime.UtcNow;
            stockBatch.UpdatedByUserId = _currentUserService.UserId.Value;

            _stockBatchRepository.Update(stockBatch);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
