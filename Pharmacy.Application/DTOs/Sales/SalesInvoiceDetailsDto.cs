using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.DTOs.Sales
{
    public class SalesInvoiceDetailsDto
    {
        public Guid SalesInvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public decimal GrandTotal { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
