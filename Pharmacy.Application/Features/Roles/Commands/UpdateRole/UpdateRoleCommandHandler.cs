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

namespace Pharmacy.Application.Features.Roles.Commands.UpdateRole
{
    public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, RoleDetailsDto>
    {
        private readonly IRepository<Role> _roleRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public UpdateRoleCommandHandler(
            IRepository<Role> roleRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _roleRepository = roleRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<RoleDetailsDto> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new BadRequestException("اسم الدور مطلوب");

            var role = await _roleRepository
                .GetAll()
                .FirstOrDefaultAsync(
                    r => r.Id == request.RoleId && !r.IsDeleted,
                    cancellationToken);

            if (role is null)
                throw new NotFoundException("Role", request.RoleId);

            var normalizedName = request.Name.Trim();

            var exists = await _roleRepository
                .GetAll()
                .AnyAsync(
                    r => r.Id != request.RoleId &&
                         !r.IsDeleted &&
                         r.Name.ToLower() == normalizedName.ToLower(),
                    cancellationToken);

            if (exists)
                throw new StatusAlreadyExistsException(request.Name);

            role.Name = normalizedName;
            role.Description = request.Description?.Trim() ?? string.Empty;
            role.UpdatedAt = DateTime.UtcNow;
            role.UpdatedByUserId = _currentUserService.UserId.Value;

            _roleRepository.Update(role);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new RoleDetailsDto
            {
                RoleId = role.Id,
                Name = role.Name,
                Description = role.Description
            };
        }
    }
}
