using MediatR;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Common.Interfaces;
using Pharmacy.Application.Common.Models;
using Pharmacy.Application.DTOs.Customers;
using Pharmacy.Domain.Entities.Partners;
using Pharmacy.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Customers.Queries.GetCustomers
{
    public class GetCustomersQueryHandler : IRequestHandler<GetCustomersQuery, PagedResult<CustomerListItemDto>>
    {
        private readonly IRepository<Customer> _customerRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetCustomersQueryHandler(
            IRepository<Customer> customerRepository,
            ICurrentUserService currentUserService)
        {
            _customerRepository = customerRepository;
            _currentUserService = currentUserService;
        }

        public async Task<PagedResult<CustomerListItemDto>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
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

            var query = _customerRepository
                .GetAll()
                .AsNoTracking()
                .Where(c => !c.IsDeleted && c.BranchId == branchId);

            query = (request.SortBy?.ToLower(), request.SortDirection?.ToLower()) switch
            {
                ("fullname", "desc") => query.OrderByDescending(c => c.FullName),
                ("phone", "desc") => query.OrderByDescending(c => c.Phone),
                ("totalpurchases", "desc") => query.OrderByDescending(c => c.TotalPurchases),
                ("debtamount", "desc") => query.OrderByDescending(c => c.DebtAmount),

                ("phone", _) => query.OrderBy(c => c.Phone),
                ("totalpurchases", _) => query.OrderBy(c => c.TotalPurchases),
                ("debtamount", _) => query.OrderBy(c => c.DebtAmount),

                _ => query.OrderBy(c => c.FullName)
            };

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(c => new CustomerListItemDto
                {
                    CustomerId = c.Id,
                    FullName = c.FullName,
                    Phone = c.Phone,
                    Address = c.Address,
                    TotalPurchases = c.TotalPurchases,
                    DebtAmount = c.DebtAmount,
                    BranchId = c.BranchId
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<CustomerListItemDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
