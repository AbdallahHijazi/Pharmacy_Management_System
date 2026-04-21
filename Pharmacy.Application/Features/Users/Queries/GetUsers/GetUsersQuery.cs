using MediatR;
using Pharmacy.Application.DTOs.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Users.Queries.GetUsers
{
    public class GetUsersQuery : IRequest<List<UserListItemDto>>
    {
    }
}
