using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.DTOs.Purchases
{
    public class CreatePurchaseReturnRequestDto
    {
        public Guid PurchaseInvoiceId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public List<CreatePurchaseReturnItemRequestDto> Items { get; set; } = new();
    }
}
