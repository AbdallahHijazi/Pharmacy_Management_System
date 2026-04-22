using MediatR;
using Pharmacy.Application.Common.Models;
using Pharmacy.Application.DTOs.Sales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Sales.Queries.GetSalesInvoices
{
    public class GetSalesInvoicesQuery : IRequest<PagedResult<SalesInvoiceListItemDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; } = "createdat";
        public string? SortDirection { get; set; } = "desc";
    }
}
