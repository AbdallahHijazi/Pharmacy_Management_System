using MediatR;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Common.Interfaces;
using Pharmacy.Application.DTOs.Suppliers;
using Pharmacy.Domain.Entities.Partners;
using Pharmacy.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Suppliers.Queries.GetSupplierById
{
    public class GetSupplierByIdQueryHandler : IRequestHandler<GetSupplierByIdQuery, SupplierDetailsDto>
    {
        private readonly IRepository<Supplier> _supplierRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetSupplierByIdQueryHandler(
            IRepository<Supplier> supplierRepository,
            ICurrentUserService currentUserService)
        {
            _supplierRepository = supplierRepository;
            _currentUserService = currentUserService;
        }

        public async Task<SupplierDetailsDto> Handle(GetSupplierByIdQuery request, CancellationToken cancellationToken)
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
