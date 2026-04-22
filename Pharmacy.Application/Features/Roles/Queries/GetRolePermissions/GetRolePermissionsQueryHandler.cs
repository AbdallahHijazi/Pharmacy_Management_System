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

namespace Pharmacy.Application.Features.Roles.Queries.GetRolePermissions
{
    public class GetRolePermissionsQueryHandler : IRequestHandler<GetRolePermissionsQuery, List<RolePermissionItemDto>>
    {
        private readonly IRepository<Role> _roleRepository;
        private readonly IRepository<RolePermission> _rolePermissionRepository;
        private readonly IRepository<Permission> _permissionRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetRolePermissionsQueryHandler(
            IRepository<Role> roleRepository,
            IRepository<RolePermission> rolePermissionRepository,
            IRepository<Permission> permissionRepository,
            ICurrentUserService currentUserService)
        {
            _roleRepository = roleRepository;
            _rolePermissionRepository = rolePermissionRepository;
            _permissionRepository = permissionRepository;
            _currentUserService = currentUserService;
        }

        public async Task<List<RolePermissionItemDto>> Handle(GetRolePermissionsQuery request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            var roleExists = await _roleRepository
                .GetAll()
                .AnyAsync(r => r.Id == request.RoleId && !r.IsDeleted, cancellationToken);

            if (!roleExists)
                throw new NotFoundException("Role", request.RoleId);

            var permissionIds = await _rolePermissionRepository
                .GetAll()
                .Where(rp => rp.RoleId == request.RoleId)
                .Select(rp => rp.PermissionId)
                .ToListAsync(cancellationToken);

            var permissions = await _permissionRepository
                .GetAll()
                .Where(p => !p.IsDeleted && permissionIds.Contains(p.Id))
                .OrderBy(p => p.Module)
                .ThenBy(p => p.Name)
                .Select(p => new RolePermissionItemDto
                {
                    PermissionId = p.Id,
                    Name = p.Name,
                    Module = p.Module
                })
                .ToListAsync(cancellationToken);

            return permissions;
        }
    }
}
