using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Roles.Commands.DeleteRole
{
    public class DeleteRoleCommand : IRequest<Unit>
    {
        public Guid RoleId { get; set; }
    }
}
