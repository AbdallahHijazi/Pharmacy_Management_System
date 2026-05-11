using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.DTOs.Purchases
{
    public class CreatePurchaseInvoiceItemRequestDto
    {
        public Guid ProductId { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        /// <summary>Paid quantity (line subtotal = this × unit price).</summary>
        public int Quantity { get; set; }
        /// <summary>Extra free units; added to stock only, not to line subtotal.</summary>
        public int BonusQuantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
