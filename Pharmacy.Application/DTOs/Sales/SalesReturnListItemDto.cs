using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.DTOs.Sales
{
    public class SalesReturnListItemDto
    {
        public Guid SalesReturnId { get; set; }
        public Guid SalesInvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public decimal RefundAmount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public Guid BranchId { get; set; }
    }
}
