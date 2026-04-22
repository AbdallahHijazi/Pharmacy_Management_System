using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.DTOs.Inventory
{
    public class AdjustStockRequestDto
    {
        public Guid StockBatchId { get; set; }
        public string Type { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
