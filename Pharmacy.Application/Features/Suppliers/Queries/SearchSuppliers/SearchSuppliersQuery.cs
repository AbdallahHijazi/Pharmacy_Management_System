using MediatR;
using Pharmacy.Application.DTOs.Suppliers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Suppliers.Queries.SearchSuppliers
{
    public class SearchSuppliersQuery : IRequest<List<SupplierListItemDto>>
    {
        public string Query { get; set; } = string.Empty;
    }
}
