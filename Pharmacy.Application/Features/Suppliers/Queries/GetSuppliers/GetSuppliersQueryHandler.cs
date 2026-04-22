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

namespace Pharmacy.Application.Features.Suppliers.Queries.GetSuppliers
{
    public class GetSuppliersQueryHandler : IRequestHandler<GetSuppliersQuery, List<SupplierListItemDto>>
    {
        private readonly IRepository<Supplier> _supplierRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetSuppliersQueryHandler(
            IRepository<Supplier> supplierRepository,
            ICurrentUserService currentUserService)
        {
            _supplierRepository = supplierRepository;
            _currentUserService = currentUserService;
        }

        public async Task<List<SupplierListItemDto>> Handle(GetSuppliersQuery request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (_currentUserService.BranchId is null)
                throw new UnauthorizedException("لا يوجد فرع مرتبط بالمستخدم الحالي");

            var suppliers = await _supplierRepository
                .GetAll()
                .Where(s => !s.IsDeleted && s.BranchId == _currentUserService.BranchId.Value)
                .OrderBy(s => s.Name)
                .Select(s => new SupplierListItemDto
                {
                    SupplierId = s.Id,
                    Name = s.Name,
                    ContactPerson = s.ContactPerson,
                    Phone = s.Phone,
                    Address = s.Address,
                    TotalPurchases = s.TotalPurchases,
                    PayableAmount = s.PayableAmount,
                    BranchId = s.BranchId
                })
                .ToListAsync(cancellationToken);

            return suppliers;
        }
    }
}
