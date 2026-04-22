using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.DTOs.Purchases
{
    public class PurchaseReturnDetailsDto
    {
        public Guid PurchaseReturnId { get; set; }
        public Guid PurchaseInvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public decimal RefundAmount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public Guid BranchId { get; set; }
    }
}
