using Pharmacy.Domain.Entities.Base;
using Pharmacy.Domain.Entities.Identity;
using Pharmacy.Domain.Entities.Organization;
using Pharmacy.Domain.Entities.Partners;
using Pharmacy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Domain.Entities.Purchases
{
    public class PurchaseInvoice : BaseEntity
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public Guid SupplierId { get; set; }
        public Guid UserId { get; set; }
        public Guid BranchId { get; set; }

        public decimal Subtotal { get; set; }
        public decimal TaxRate { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }

        public PaymentMethod PaymentMethod { get; set; }
        public PurchaseInvoiceStatus Status { get; set; } = PurchaseInvoiceStatus.Received;

        public Supplier Supplier { get; set; } = null!;
        public User User { get; set; } = null!;
        public Branch Branch { get; set; } = null!;

        public ICollection<PurchaseInvoiceItem> Items { get; set; } = new List<PurchaseInvoiceItem>();
        public ICollection<PurchaseReturn> Returns { get; set; } = new List<PurchaseReturn>();
    }
}
