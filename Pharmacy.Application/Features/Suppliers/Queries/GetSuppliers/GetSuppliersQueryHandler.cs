using MediatR;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Common.Interfaces;
using Pharmacy.Application.Common.Models;
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
    public class GetSuppliersQueryHandler : IRequestHandler<GetSuppliersQuery, PagedResult<SupplierListItemDto>>
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

        public async Task<PagedResult<SupplierListItemDto>> Handle(GetSuppliersQuery request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (_currentUserService.BranchId is null)
                throw new UnauthorizedException("لا يوجد فرع مرتبط بالمستخدم الحالي");

            if (request.PageNumber <= 0)
                throw new BadRequestException("رقم الصفحة يجب أن يكون أكبر من صفر");

            if (request.PageSize <= 0 || request.PageSize > 100)
                throw new BadRequestException("حجم الصفحة يجب أن يكون بين 1 و 100");

            var branchId = _currentUserService.BranchId.Value;

            var query = _supplierRepository
                .GetAll()
                .AsNoTracking()
                .Where(s => !s.IsDeleted && s.BranchId == branchId);

            query = (request.SortBy?.ToLower(), request.SortDirection?.ToLower()) switch
            {
                ("name", "desc") => query.OrderByDescending(s => s.Name),
                ("contactperson", "desc") => query.OrderByDescending(s => s.ContactPerson),
                ("phone", "desc") => query.OrderByDescending(s => s.Phone),
                ("totalpurchases", "desc") => query.OrderByDescending(s => s.TotalPurchases),
                ("payableamount", "desc") => query.OrderByDescending(s => s.PayableAmount),

                ("contactperson", _) => query.OrderBy(s => s.ContactPerson),
                ("phone", _) => query.OrderBy(s => s.Phone),
                ("totalpurchases", _) => query.OrderBy(s => s.TotalPurchases),
                ("payableamount", _) => query.OrderBy(s => s.PayableAmount),

                _ => query.OrderBy(s => s.Name)
            };

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
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

            return new PagedResult<SupplierListItemDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}

