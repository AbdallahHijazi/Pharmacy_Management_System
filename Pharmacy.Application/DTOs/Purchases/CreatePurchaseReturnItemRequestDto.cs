using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.DTOs.Purchases
{
    public class CreatePurchaseReturnItemRequestDto
    {
        public Guid StockBatchId { get; set; }
        public int Quantity { get; set; }
    }
}
