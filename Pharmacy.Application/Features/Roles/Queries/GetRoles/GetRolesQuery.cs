using MediatR;
using Pharmacy.Application.DTOs.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Roles.Queries.GetRoles
{
    public class GetRolesQuery : IRequest<List<RoleListItemDto>>
    {
    }
}
