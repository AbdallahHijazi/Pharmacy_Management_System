using Pharmacy.Domain.Enums;

namespace Pharmacy.Application.DTOs.Products
{
    public class CreateProductRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public string ScientificName { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
        public decimal SellingPrice { get; set; }
        public ProductPricingType PricingType { get; set; } = ProductPricingType.FreePricingMedicine;
        /// <summary>سعر الشراء المرجعي للبطاقة (وطني: مطلوب؛ حر: اختياري).</summary>
        public decimal? PurchasePrice { get; set; }
        public Guid? DefaultSupplierId { get; set; }
    }
}
