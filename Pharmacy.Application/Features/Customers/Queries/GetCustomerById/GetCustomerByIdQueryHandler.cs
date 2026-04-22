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

namespace Pharmacy.Application.Features.Customers.Queries.GetCustomerById
{
    public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, CustomerDetailsDto>
    {
        private readonly IRepository<Customer> _customerRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetCustomerByIdQueryHandler(
            IRepository<Customer> customerRepository,
            ICurrentUserService currentUserService)
        {
            _customerRepository = customerRepository;
            _currentUserService = currentUserService;
        }

        public async Task<CustomerDetailsDto> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (_currentUserService.BranchId is null)
                throw new UnauthorizedException("لا يوجد فرع مرتبط بالمستخدم الحالي");

            var customer = await _customerRepository
                .GetAllAsNoTracking()
                .FirstOrDefaultAsync(
                    c => c.Id == request.CustomerId &&
                         !c.IsDeleted &&
                         c.BranchId == _currentUserService.BranchId.Value,
                    cancellationToken);

            if (customer is null)
                throw new NotFoundException("Customer", request.CustomerId);

            return new CustomerDetailsDto
            {
                CustomerId = customer.Id,
                FullName = customer.FullName,
                Phone = customer.Phone,
                Address = customer.Address,
                TotalPurchases = customer.TotalPurchases,
                DebtAmount = customer.DebtAmount,
                BranchId = customer.BranchId
            };
        }
    }
}
