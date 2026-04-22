using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.DTOs.Sales
{
    public class CreateSalesInvoiceRequestDto
    {
        public Guid? CustomerId { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal PaidAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public List<CreateSalesInvoiceItemRequestDto> Items { get; set; } = new();
    }
}
