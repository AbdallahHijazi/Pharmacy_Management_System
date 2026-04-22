using MediatR;
using Pharmacy.Application.DTOs.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Roles.Queries.GetRolePermissions
{
    public class GetRolePermissionsQuery : IRequest<List<RolePermissionItemDto>>
    {
        public Guid RoleId { get; set; }
    }
}
