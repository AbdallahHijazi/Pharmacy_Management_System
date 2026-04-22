using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.DTOs.Sales
{
    public class CreateSalesReturnRequestDto
    {
        public Guid SalesInvoiceId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public List<CreateSalesReturnItemRequestDto> Items { get; set; } = new();
    }
}
