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

namespace Pharmacy.Application.Features.Users.Commands.UpdateUser
{
    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserDetailsDto>
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Role> _roleRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public UpdateUserCommandHandler(
            IRepository<User> userRepository,
            IRepository<Role> roleRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<UserDetailsDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (_currentUserService.BranchId is null)
                throw new UnauthorizedException("لا يوجد فرع مرتبط بالمستخدم الحالي");

            var user = await _userRepository
                .GetAll()
                .FirstOrDefaultAsync(
                    u => u.Id == request.UserId &&
                         !u.IsDeleted &&
                         u.BranchId == _currentUserService.BranchId.Value,
                    cancellationToken);

            if (user is null)
                throw new NotFoundException("User", request.UserId);

            var normalizedEmail = request.Email.Trim().ToLower();

            var emailExists = await _userRepository
                .GetAll()
                .AnyAsync(
                    u => u.Id != request.UserId &&
                         !u.IsDeleted &&
                         u.Email.ToLower() == normalizedEmail,
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

            user.FullName = request.FullName.Trim();
            user.Email = normalizedEmail;
            user.Phone = request.Phone?.Trim() ?? string.Empty;
            user.RoleId = request.RoleId;
            user.IsActive = request.IsActive;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedByUserId = _currentUserService.UserId.Value;

            _userRepository.Update(user);
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
