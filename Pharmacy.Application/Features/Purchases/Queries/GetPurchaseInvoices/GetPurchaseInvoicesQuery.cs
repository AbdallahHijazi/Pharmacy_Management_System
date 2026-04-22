using MediatR;
using Pharmacy.Application.Common.Models;
using Pharmacy.Application.DTOs.Purchases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Purchases.Queries.GetPurchaseInvoices
{
    public class GetPurchaseInvoicesQuery : IRequest<PagedResult<PurchaseInvoiceListItemDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; } = "createdat";
        public string? SortDirection { get; set; } = "desc";
    }
}
