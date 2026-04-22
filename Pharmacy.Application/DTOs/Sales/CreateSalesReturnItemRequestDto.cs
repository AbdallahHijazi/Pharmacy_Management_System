using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.DTOs.Sales
{
    public class CreateSalesReturnItemRequestDto
    {
        public Guid SalesInvoiceItemId { get; set; }
        public int Quantity { get; set; }
    }
}
