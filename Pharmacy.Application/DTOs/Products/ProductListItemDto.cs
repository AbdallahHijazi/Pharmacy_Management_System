using Pharmacy.Domain.Enums;

namespace Pharmacy.Application.DTOs.Products
{
    public class ProductListItemDto
    {
        public Guid ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ScientificName { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal SellingPrice { get; set; }
        public ProductPricingType PricingType { get; set; }
        public decimal? PurchasePrice { get; set; }
        public decimal? CalculatedUnitProfit { get; set; }
        public Guid? DefaultSupplierId { get; set; }
        public Guid BranchId { get; set; }
        /// <summary>مجموع الكمية المتاحة من كل الدفعات النشطة في الفرع.</summary>
        public int TotalAvailableQuantity { get; set; }
        /// <summary>وحدات المتاح غير المنتهية صلاحيتها (حسب تاريخ الصلاحية مقابل الوقت الحالي UTC).</summary>
        public int SellableQuantity { get; set; }
        /// <summary>وحدات المتاح في دفعات منتهية الصلاحية (لا تُحسب ضمن البيع).</summary>
        public int ExpiredQuantity { get; set; }
    }
}
