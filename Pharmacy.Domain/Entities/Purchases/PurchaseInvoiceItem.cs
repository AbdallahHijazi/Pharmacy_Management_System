using Pharmacy.Domain.Entities.Base;
using Pharmacy.Domain.Entities.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Domain.Entities.Purchases
{
    public class PurchaseInvoiceItem : BaseEntity
    {
        public Guid PurchaseInvoiceId { get; set; }
        public Guid ProductId { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        /// <summary>Paid / invoiced quantity (line total uses this × <see cref="UnitPrice"/>).</summary>
        public int Quantity { get; set; }
        /// <summary>Free bonus units received with the line; not included in line total.</summary>
        public int BonusQuantity { get; set; }
        public decimal UnitPrice { get; set; }

        public PurchaseInvoice PurchaseInvoice { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}
