using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.DTOs.Inventory
{
    public class StockBatchDetailsDto
    {
        public Guid StockBatchId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string LotNumber { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public decimal PurchasePrice { get; set; }
        public int ReceivedQuantity { get; set; }
        public int AvailableQuantity { get; set; }
        public Guid SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public Guid BranchId { get; set; }
    }
}
