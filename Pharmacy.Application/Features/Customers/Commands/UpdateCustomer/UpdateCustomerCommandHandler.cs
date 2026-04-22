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

namespace Pharmacy.Application.Features.Customers.Commands.UpdateCustomer
{
    public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, CustomerDetailsDto>
    {
        private readonly IRepository<Customer> _customerRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public UpdateCustomerCommandHandler(
            IRepository<Customer> customerRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _customerRepository = customerRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<CustomerDetailsDto> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (_currentUserService.BranchId is null)
                throw new UnauthorizedException("لا يوجد فرع مرتبط بالمستخدم الحالي");

            if (string.IsNullOrWhiteSpace(request.FullName))
                throw new BadRequestException("اسم الزبون مطلوب");

            var customer = await _customerRepository
                .GetAll()
                .FirstOrDefaultAsync(
                    c => c.Id == request.CustomerId &&
                         !c.IsDeleted &&
                         c.BranchId == _currentUserService.BranchId.Value,
                    cancellationToken);

            if (customer is null)
                throw new NotFoundException("Customer", request.CustomerId);

            customer.FullName = request.FullName.Trim();
            customer.Phone = request.Phone?.Trim() ?? string.Empty;
            customer.Address = request.Address?.Trim() ?? string.Empty;
            customer.UpdatedAt = DateTime.UtcNow;
            customer.UpdatedByUserId = _currentUserService.UserId.Value;

            _customerRepository.Update(customer);
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
