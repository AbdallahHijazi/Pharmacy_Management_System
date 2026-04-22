using MediatR;
using Pharmacy.Application.Common.Interfaces;
using Pharmacy.Application.DTOs.Customers;
using Pharmacy.Domain.Entities.Partners;
using Pharmacy.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Customers.Commands.CreateCustomer
{
    public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, CustomerDetailsDto>
    {
        private readonly IRepository<Customer> _customerRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public CreateCustomerCommandHandler(
            IRepository<Customer> customerRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _customerRepository = customerRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<CustomerDetailsDto> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (_currentUserService.BranchId is null)
                throw new UnauthorizedException("لا يوجد فرع مرتبط بالمستخدم الحالي");

            if (string.IsNullOrWhiteSpace(request.FullName))
                throw new BadRequestException("اسم الزبون مطلوب");

            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                FullName = request.FullName.Trim(),
                Phone = request.Phone?.Trim() ?? string.Empty,
                Address = request.Address?.Trim() ?? string.Empty,
                TotalPurchases = 0,
                DebtAmount = 0,
                BranchId = _currentUserService.BranchId.Value,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = _currentUserService.UserId.Value,
                IsDeleted = false
            };

            _customerRepository.Add(customer);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

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
