using MediatR;
using Pharmacy.Application.DTOs.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Roles.Commands.UpdateRole
{
    public class UpdateRoleCommand : IRequest<RoleDetailsDto>
    {
        public Guid RoleId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
