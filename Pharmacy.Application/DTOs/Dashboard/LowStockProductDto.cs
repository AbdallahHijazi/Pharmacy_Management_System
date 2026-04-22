using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.DTOs.Dashboard
{
    public class LowStockProductDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int TotalAvailableQuantity { get; set; }
        public int BatchesCount { get; set; }
    }
}
