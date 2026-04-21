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

namespace Pharmacy.Application.Features.Users.Queries.GetUserById
{
    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDetailsDto>
    {
        private readonly IRepository<User> _userRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetUserByIdQueryHandler(
            IRepository<User> userRepository,
            ICurrentUserService currentUserService)
        {
            _userRepository = userRepository;
            _currentUserService = currentUserService;
        }

        public async Task<UserDetailsDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (_currentUserService.BranchId is null)
                throw new UnauthorizedException("لا يوجد فرع مرتبط بالمستخدم الحالي");

            var user = await _userRepository
                .GetAll()
                .Include(u => u.Role)
                .FirstOrDefaultAsync(
                    u => u.Id == request.UserId &&
                         !u.IsDeleted &&
                         u.BranchId == _currentUserService.BranchId.Value,
                    cancellationToken);

            if (user is null)
                throw new NotFoundException("User", request.UserId);

            return new UserDetailsDto
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role.Name,
                RoleId = user.RoleId,
                BranchId = user.BranchId,
                IsActive = user.IsActive,
                LastLoginAt = user.LastLoginAt
            };
        }
    }
}
