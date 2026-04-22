using MediatR;
using Pharmacy.Application.Common.Models;
using Pharmacy.Application.DTOs.Suppliers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Suppliers.Queries.GetSuppliers
{
    public class GetSuppliersQuery : IRequest<PagedResult<SupplierListItemDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; } = "name";
        public string? SortDirection { get; set; } = "asc";
    }
}
