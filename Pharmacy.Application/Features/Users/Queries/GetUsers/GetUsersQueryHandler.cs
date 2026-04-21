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

namespace Pharmacy.Application.Features.Users.Queries.GetUsers
{
    public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, List<UserListItemDto>>
    {
        private readonly IRepository<User> _userRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetUsersQueryHandler(
            IRepository<User> userRepository,
            ICurrentUserService currentUserService)
        {
            _userRepository = userRepository;
            _currentUserService = currentUserService;
        }

        public async Task<List<UserListItemDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (_currentUserService.BranchId is null)
                throw new UnauthorizedException("لا يوجد فرع مرتبط بالمستخدم الحالي");

            var users = await _userRepository
                .GetAll()
                .Include(u => u.Role)
                .Where(u => !u.IsDeleted && u.BranchId == _currentUserService.BranchId.Value)
                .OrderBy(u => u.FullName)
                .Select(u => new UserListItemDto
                {
                    UserId = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    Phone = u.Phone,
                    Role = u.Role.Name,
                    BranchId = u.BranchId,
                    IsActive = u.IsActive,
                    LastLoginAt = u.LastLoginAt
                })
                .ToListAsync(cancellationToken);

            return users;
        }
    }
}
