using MediatR;
using Pharmacy.Application.DTOs.Customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Customers.Queries.SearchCustomers
{
    public class SearchCustomersQuery : IRequest<List<CustomerListItemDto>>
    {
        public string Query { get; set; } = string.Empty;
    }
}
