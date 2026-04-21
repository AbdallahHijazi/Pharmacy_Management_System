using Pharmacy.Domain.Entities.Base;
using Pharmacy.Domain.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Domain.Entities.Purchases
{
    public class PurchaseReturn : BaseEntity
    {
        public Guid PurchaseInvoiceId { get; set; }
        public Guid UserId { get; set; }
        public decimal RefundAmount { get; set; }
        public string Reason { get; set; } = string.Empty;

        public PurchaseInvoice PurchaseInvoice { get; set; } = null!;
        public User User { get; set; } = null!;
        public ICollection<PurchaseReturnItem> Items { get; set; } = new List<PurchaseReturnItem>();
    }
}
