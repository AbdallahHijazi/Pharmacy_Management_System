using MediatR;
using Pharmacy.Application.DTOs.Products;
using Pharmacy.Domain.Enums;

namespace Pharmacy.Application.Features.Products.Commands.CreateProduct
{
    public class CreateProductCommand : IRequest<ProductDetailsDto>
    {
        public string Name { get; set; } = string.Empty;
        public string ScientificName { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
        public decimal SellingPrice { get; set; }
        public ProductPricingType PricingType { get; set; } = ProductPricingType.FreePricingMedicine;
        public decimal? PurchasePrice { get; set; }
        public Guid? DefaultSupplierId { get; set; }
    }
}
