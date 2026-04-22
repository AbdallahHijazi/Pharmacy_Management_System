using MediatR;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Common.Interfaces;
using Pharmacy.Application.DTOs.Customers;
using Pharmacy.Domain.Entities.Partners;
using Pharmacy.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Customers.Queries.SearchCustomers
{
    public class SearchCustomersQueryHandler : IRequestHandler<SearchCustomersQuery, List<CustomerListItemDto>>
    {
        private readonly IRepository<Customer> _customerRepository;
        private readonly ICurrentUserService _currentUserService;

        public SearchCustomersQueryHandler(
            IRepository<Customer> customerRepository,
            ICurrentUserService currentUserService)
        {
            _customerRepository = customerRepository;
            _currentUserService = currentUserService;
        }

        public async Task<List<CustomerListItemDto>> Handle(SearchCustomersQuery request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (_currentUserService.BranchId is null)
                throw new UnauthorizedException("لا يوجد فرع مرتبط بالمستخدم الحالي");

            var query = request.Query?.Trim();

            if (string.IsNullOrWhiteSpace(query))
                return new List<CustomerListItemDto>();

            var normalizedQuery = query.ToLower();
            var branchId = _currentUserService.BranchId.Value;

            var customers = await _customerRepository
                .GetAllAsNoTracking()
                .Where(c => !c.IsDeleted &&
                            c.BranchId == branchId &&
                            (
                                c.FullName.ToLower().Contains(normalizedQuery) ||
                                c.Phone.ToLower().Contains(normalizedQuery) ||
                                c.Address.ToLower().Contains(normalizedQuery)
                            ))
                .OrderBy(c => c.FullName)
                .Take(20)
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

            return customers;
        }
    }
}
