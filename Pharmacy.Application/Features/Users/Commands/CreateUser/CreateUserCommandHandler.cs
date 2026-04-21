using MediatR;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Common.Interfaces;
using Pharmacy.Application.DTOs.Users;
using Pharmacy.Domain.Entities.Identity;
using Pharmacy.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Users.Commands.CreateUser
{
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserDetailsDto>
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Role> _roleRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUserPasswordHasher _passwordHasher;

        public CreateUserCommandHandler(
            IRepository<User> userRepository,
            IRepository<Role> roleRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IUserPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _passwordHasher = passwordHasher;
        }

        public async Task<UserDetailsDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (_currentUserService.BranchId is null)
                throw new UnauthorizedException("لا يوجد فرع مرتبط بالمستخدم الحالي");

            if (string.IsNullOrWhiteSpace(request.FullName))
                throw new BadRequestException("الاسم الكامل مطلوب");

            if (string.IsNullOrWhiteSpace(request.Email))
                throw new BadRequestException("البريد الإلكتروني مطلوب");

            if (string.IsNullOrWhiteSpace(request.Password))
                throw new BadRequestException("كلمة المرور مطلوبة");

            var normalizedEmail = request.Email.Trim().ToLower();

            var emailExists = await _userRepository
                .GetAll()
                .AnyAsync(
                    u => !u.IsDeleted && u.Email.ToLower() == normalizedEmail,
                    cancellationToken);

            if (emailExists)
                throw new StatusAlreadyExistsException(request.Email);

            var role = await _roleRepository
                .GetAll()
                .FirstOrDefaultAsync(
                    r => r.Id == request.RoleId && !r.IsDeleted,
                    cancellationToken);

            if (role is null)
                throw new NotFoundException("Role", request.RoleId);

            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = request.FullName.Trim(),
                Email = normalizedEmail,
                Phone = request.Phone?.Trim() ?? string.Empty,
                PasswordHash = _passwordHasher.Hash(request.Password),
                RoleId = request.RoleId,
                BranchId = _currentUserService.BranchId.Value,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = _currentUserService.UserId.Value,
                IsDeleted = false
            };

            _userRepository.Add(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new UserDetailsDto
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Role = role.Name,
                RoleId = user.RoleId,
                BranchId = user.BranchId,
                IsActive = user.IsActive,
                LastLoginAt = user.LastLoginAt
            };
        }
    }
}
