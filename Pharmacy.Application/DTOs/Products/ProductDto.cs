using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.DTOs.Products
{
    public class ProductDto
    {
        public Guid ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ScientificName { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal SellingPrice { get; set; }
        public Guid? DefaultSupplierId { get; set; }
        public int TotalQuantity { get; set; }
        public int ExpiredQuantity { get; set; }
        public int SellableQuantity { get; set; }
        public Guid BranchId { get; set; }
    }
}
