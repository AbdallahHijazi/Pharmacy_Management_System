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

namespace Pharmacy.Application.Features.Roles.Queries.GetRoles
{
    public class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, List<RoleListItemDto>>
    {
        private readonly IRepository<Role> _roleRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetRolesQueryHandler(
            IRepository<Role> roleRepository,
            ICurrentUserService currentUserService)
        {
            _roleRepository = roleRepository;
            _currentUserService = currentUserService;
        }

        public async Task<List<RoleListItemDto>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            var roles = await _roleRepository
                .GetAll()
                .Where(r => !r.IsDeleted)
                .OrderBy(r => r.Name)
                .Select(r => new RoleListItemDto
                {
                    RoleId = r.Id,
                    Name = r.Name,
                    Description = r.Description
                })
                .ToListAsync(cancellationToken);

            return roles;
        }
    }
}
