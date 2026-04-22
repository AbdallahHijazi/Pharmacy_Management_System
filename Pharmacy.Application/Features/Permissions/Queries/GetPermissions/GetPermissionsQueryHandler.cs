using MediatR;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Common.Interfaces;
using Pharmacy.Application.DTOs.Permissions;
using Pharmacy.Domain.Entities.Identity;
using Pharmacy.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Permissions.Queries.GetPermissions
{
    public class GetPermissionsQueryHandler : IRequestHandler<GetPermissionsQuery, List<PermissionListItemDto>>
    {
        private readonly IRepository<Permission> _permissionRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetPermissionsQueryHandler(
            IRepository<Permission> permissionRepository,
            ICurrentUserService currentUserService)
        {
            _permissionRepository = permissionRepository;
            _currentUserService = currentUserService;
        }

        public async Task<List<PermissionListItemDto>> Handle(GetPermissionsQuery request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            var permissions = await _permissionRepository
                .GetAll()
                .Where(p => !p.IsDeleted)
                .OrderBy(p => p.Module)
                .ThenBy(p => p.Name)
                .Select(p => new PermissionListItemDto
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
