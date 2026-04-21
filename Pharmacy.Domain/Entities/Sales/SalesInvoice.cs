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

namespace Pharmacy.Domain.Entities.Sales
{
    public class SalesInvoice : BaseEntity
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public Guid? CustomerId { get; set; }
        public Guid UserId { get; set; }
        public Guid BranchId { get; set; }

        public decimal Subtotal { get; set; }
        public decimal DiscountPercentage { get; set; } = 0;
        public decimal DiscountAmount { get; set; } = 0;
        public decimal TaxRate { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }

        public PaymentMethod PaymentMethod { get; set; }
        public SalesInvoiceStatus Status { get; set; } = SalesInvoiceStatus.Completed;

        public Customer? Customer { get; set; }
        public User User { get; set; } = null!;
        public Branch Branch { get; set; } = null!;

        public ICollection<SalesInvoiceItem> Items { get; set; } = new List<SalesInvoiceItem>();
        public ICollection<SalesReturn> Returns { get; set; } = new List<SalesReturn>();
    }
}
