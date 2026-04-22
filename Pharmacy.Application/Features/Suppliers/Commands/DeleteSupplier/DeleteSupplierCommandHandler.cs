using MediatR;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Common.Interfaces;
using Pharmacy.Domain.Entities.Partners;
using Pharmacy.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Suppliers.Commands.DeleteSupplier
{
    public class DeleteSupplierCommandHandler : IRequestHandler<DeleteSupplierCommand, Unit>
    {
        private readonly IRepository<Supplier> _supplierRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public DeleteSupplierCommandHandler(
            IRepository<Supplier> supplierRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _supplierRepository = supplierRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Unit> Handle(DeleteSupplierCommand request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (_currentUserService.BranchId is null)
                throw new UnauthorizedException("لا يوجد فرع مرتبط بالمستخدم الحالي");

            var supplier = await _supplierRepository
                .GetAll()
                .FirstOrDefaultAsync(
                    s => s.Id == request.SupplierId &&
                         !s.IsDeleted &&
                         s.BranchId == _currentUserService.BranchId.Value,
                    cancellationToken);

            if (supplier is null)
                throw new NotFoundException("Supplier", request.SupplierId);

            supplier.IsDeleted = true;
            supplier.DeletedAt = DateTime.UtcNow;
            supplier.UpdatedAt = DateTime.UtcNow;
            supplier.UpdatedByUserId = _currentUserService.UserId.Value;

            _supplierRepository.Update(supplier);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
