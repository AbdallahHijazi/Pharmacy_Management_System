using MediatR;
using Pharmacy.Application.DTOs.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Permissions.Queries.GetPermissions
{
    public class GetPermissionsQuery : IRequest<List<PermissionListItemDto>>
    {
    }
}
