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
        /// <summary>مجموع المتاح من الدفعات النشطة في الفرع.</summary>
        public int TotalAvailableQuantity { get; set; }
        public int SellableQuantity { get; set; }
        /// <summary>متاح لكن منتهي الصلاحية — لا يُدخل في الكمية القابلة للبيع.</summary>
        public int ExpiredQuantity { get; set; }
        public Guid BranchId { get; set; }
    }
}
