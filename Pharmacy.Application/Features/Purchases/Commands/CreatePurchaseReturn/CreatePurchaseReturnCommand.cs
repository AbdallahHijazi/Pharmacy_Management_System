using MediatR;
using Pharmacy.Application.DTOs.Purchases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Purchases.Commands.CreatePurchaseReturn
{
    public class CreatePurchaseReturnCommand : IRequest<PurchaseReturnDetailsDto>
    {
        public Guid PurchaseInvoiceId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public List<CreatePurchaseReturnItemRequestDto> Items { get; set; } = new();
    }
}
