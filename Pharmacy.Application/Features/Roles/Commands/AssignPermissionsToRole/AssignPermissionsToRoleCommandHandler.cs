using MediatR;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Common.Interfaces;
using Pharmacy.Domain.Entities.Identity;
using Pharmacy.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Roles.Commands.AssignPermissionsToRole
{
    public class AssignPermissionsToRoleCommandHandler : IRequestHandler<AssignPermissionsToRoleCommand, Unit>
    {
        private readonly IRepository<Role> _roleRepository;
        private readonly IRepository<Permission> _permissionRepository;
        private readonly IRepository<RolePermission> _rolePermissionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public AssignPermissionsToRoleCommandHandler(
            IRepository<Role> roleRepository,
            IRepository<Permission> permissionRepository,
            IRepository<RolePermission> rolePermissionRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _roleRepository = roleRepository;
            _permissionRepository = permissionRepository;
            _rolePermissionRepository = rolePermissionRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Unit> Handle(AssignPermissionsToRoleCommand request, CancellationToken cancellationToken)
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

            var permissionIds = request.PermissionIds
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList();

            var existingPermissions = await _permissionRepository
                .GetAll()
                .Where(p => !p.IsDeleted && permissionIds.Contains(p.Id))
                .Select(p => p.Id)
                .ToListAsync(cancellationToken);

            var invalidPermissionIds = permissionIds.Except(existingPermissions).ToList();

            if (invalidPermissionIds.Any())
                throw new NotFoundException("Permission", invalidPermissionIds.First());

            var currentRolePermissions = await _rolePermissionRepository
                .GetAll()
                .Where(rp => rp.RoleId == request.RoleId)
                .ToListAsync(cancellationToken);

            foreach (var rolePermission in currentRolePermissions)
            {
                _rolePermissionRepository.Delete(rolePermission);
            }

            foreach (var permissionId in permissionIds)
            {
                _rolePermissionRepository.Add(new RolePermission
                {
                    RoleId = request.RoleId,
                    PermissionId = permissionId
                });
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
