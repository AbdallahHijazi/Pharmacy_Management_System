using MediatR;
using Pharmacy.Application.Common.Models;
using Pharmacy.Application.DTOs.Customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Customers.Queries.GetCustomers
{
    public class GetCustomersQuery : IRequest<PagedResult<CustomerListItemDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; } = "fullname";
        public string? SortDirection { get; set; } = "asc";
    }
}
