using Pharmacy.Domain.Enums;

namespace Pharmacy.Application.DTOs.Products
{
    public class ProductDetailsDto
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
        public int TotalAvailableQuantity { get; set; }
        public int SellableQuantity { get; set; }
        public int ExpiredQuantity { get; set; }
        public Guid? DefaultSupplierId { get; set; }
        public Guid BranchId { get; set; }
    }
}
