using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.DTOs.Roles
{
    public class AssignPermissionsToRoleRequestDto
    {
        public List<Guid> PermissionIds { get; set; } = new();
    }
}
