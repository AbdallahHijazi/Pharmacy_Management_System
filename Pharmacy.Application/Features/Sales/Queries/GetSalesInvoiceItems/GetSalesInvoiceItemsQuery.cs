using MediatR;
using Pharmacy.Application.DTOs.Sales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Sales.Queries.GetSalesInvoiceItems
{
    public class GetSalesInvoiceItemsQuery : IRequest<List<SalesInvoiceItemDto>>
    {
        public Guid SalesInvoiceId { get; set; }
    }
}
