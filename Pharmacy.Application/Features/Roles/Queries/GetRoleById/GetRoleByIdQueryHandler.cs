using MediatR;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Common.Interfaces;
using Pharmacy.Application.DTOs.Roles;
using Pharmacy.Domain.Entities.Identity;
using Pharmacy.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Roles.Queries.GetRoleById
{
    public class GetRoleByIdQueryHandler : IRequestHandler<GetRoleByIdQuery, RoleDetailsDto>
    {
        private readonly IRepository<Role> _roleRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetRoleByIdQueryHandler(
            IRepository<Role> roleRepository,
            ICurrentUserService currentUserService)
        {
            _roleRepository = roleRepository;
            _currentUserService = currentUserService;
        }

        public async Task<RoleDetailsDto> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            var role = await _roleRepository
                .GetAll()
                .FirstOrDefaultAsync(
                    r => r.Id == request.RoleId && !r.IsDeleted,
                    cancellationToken);

            if (role is null)
                throw new NotFoundException("Role", request.RoleId);

            return new RoleDetailsDto
            {
                RoleId = role.Id,
                Name = role.Name,
                Description = role.Description
            };
        }
    }
}
