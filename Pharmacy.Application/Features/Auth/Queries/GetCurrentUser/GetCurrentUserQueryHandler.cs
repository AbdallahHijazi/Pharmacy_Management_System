using MediatR;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Common.Interfaces;
using Pharmacy.Application.DTOs.Auth;
using Pharmacy.Domain.Entities.Identity;
using Pharmacy.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Auth.Queries.GetCurrentUser
{
    public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, CurrentUserDto>
    {
        private readonly IRepository<User> _userRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetCurrentUserQueryHandler(
            IRepository<User> userRepository,
            ICurrentUserService currentUserService)
        {
            _userRepository = userRepository;
            _currentUserService = currentUserService;
        }

        public async Task<CurrentUserDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            var user = await _userRepository.GetAll()
                                            .Include(u => u.Role)
                                            .FirstOrDefaultAsync
                                            (
                                                u => u.Id == _currentUserService.UserId.Value && !u.IsDeleted,
                                                cancellationToken
                                            );

            if (user is null)
                throw new UnauthorizedException("المستخدم الحالي غير موجود");

            return new CurrentUserDto
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role?.Name ?? string.Empty,
                BranchId = user.BranchId,
                IsActive = user.IsActive
            };
        }
    }
}
