using MediatR;
using Pharmacy.Application.DTOs.Purchases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Purchases.Queries.GetPurchaseInvoices
{
    public class GetPurchaseInvoicesQuery : IRequest<List<PurchaseInvoiceListItemDto>>
    {
    }
}
