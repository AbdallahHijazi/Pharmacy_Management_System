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

namespace Pharmacy.Application.Features.Customers.Queries.GetCustomers
{
    public class GetCustomersQueryHandler : IRequestHandler<GetCustomersQuery, List<CustomerListItemDto>>
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

        public async Task<List<CustomerListItemDto>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (_currentUserService.BranchId is null)
                throw new UnauthorizedException("لا يوجد فرع مرتبط بالمستخدم الحالي");

            var customers = await _customerRepository
                .GetAll()
                .Where(c => !c.IsDeleted && c.BranchId == _currentUserService.BranchId.Value)
                .OrderBy(c => c.FullName)
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
