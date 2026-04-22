using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.DTOs.Sales
{
    public class CreateSalesInvoiceItemRequestDto
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
