using MediatR;
using Pharmacy.Application.DTOs.Customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Customers.Queries.GetCustomers
{
    public class GetCustomersQuery : IRequest<List<CustomerListItemDto>>
    {
    }
}
