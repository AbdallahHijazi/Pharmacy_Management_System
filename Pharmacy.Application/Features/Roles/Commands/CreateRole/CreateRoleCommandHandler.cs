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

namespace Pharmacy.Application.Features.Roles.Commands.CreateRole
{
    public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, RoleDetailsDto>
    {
        private readonly IRepository<Role> _roleRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public CreateRoleCommandHandler(
            IRepository<Role> roleRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _roleRepository = roleRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<RoleDetailsDto> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new BadRequestException("اسم الدور مطلوب");

            var normalizedName = request.Name.Trim();

            var exists = await _roleRepository
                .GetAll()
                .AnyAsync(
                    r => !r.IsDeleted && r.Name.ToLower() == normalizedName.ToLower(),
                    cancellationToken);

            if (exists)
                throw new StatusAlreadyExistsException(request.Name);

            var role = new Role
            {
                Id = Guid.NewGuid(),
                Name = normalizedName,
                Description = request.Description?.Trim() ?? string.Empty,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = _currentUserService.UserId.Value,
                IsDeleted = false
            };

            _roleRepository.Add(role);
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
