using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.DTOs.Reports
{
    public class InventoryReportDto
    {
        public int TotalProductsInStock { get; set; }
        public int TotalAvailableQuantity { get; set; }
        public int LowStockBatchesCount { get; set; }
        public int ExpiringSoonBatchesCount { get; set; }
        public int ExpiredBatchesCount { get; set; }
    }
}
