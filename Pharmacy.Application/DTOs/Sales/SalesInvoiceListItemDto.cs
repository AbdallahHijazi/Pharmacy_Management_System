using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.DTOs.Sales
{
    public class SalesInvoiceListItemDto
    {
        public Guid SalesInvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public Guid? CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public Guid BranchId { get; set; }
        public decimal Subtotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
