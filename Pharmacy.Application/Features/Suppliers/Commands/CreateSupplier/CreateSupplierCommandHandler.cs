using MediatR;
using Pharmacy.Application.Common.Interfaces;
using Pharmacy.Application.DTOs.Suppliers;
using Pharmacy.Domain.Entities.Partners;
using Pharmacy.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Suppliers.Commands.CreateSupplier
{
    public class CreateSupplierCommandHandler : IRequestHandler<CreateSupplierCommand, SupplierDetailsDto>
    {
        private readonly IRepository<Supplier> _supplierRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public CreateSupplierCommandHandler(
            IRepository<Supplier> supplierRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _supplierRepository = supplierRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<SupplierDetailsDto> Handle(CreateSupplierCommand request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (_currentUserService.BranchId is null)
                throw new UnauthorizedException("لا يوجد فرع مرتبط بالمستخدم الحالي");

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new BadRequestException("اسم المورد مطلوب");

            var supplier = new Supplier
            {
                Id = Guid.NewGuid(),
                Name = request.Name.Trim(),
                ContactPerson = request.ContactPerson?.Trim() ?? string.Empty,
                Phone = request.Phone?.Trim() ?? string.Empty,
                Address = request.Address?.Trim() ?? string.Empty,
                TotalPurchases = 0,
                PayableAmount = 0,
                BranchId = _currentUserService.BranchId.Value,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = _currentUserService.UserId.Value,
                IsDeleted = false
            };

            _supplierRepository.Add(supplier);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new SupplierDetailsDto
            {
                SupplierId = supplier.Id,
                Name = supplier.Name,
                ContactPerson = supplier.ContactPerson,
                Phone = supplier.Phone,
                Address = supplier.Address,
                TotalPurchases = supplier.TotalPurchases,
                PayableAmount = supplier.PayableAmount,
                BranchId = supplier.BranchId
            };
        }
    }
}
