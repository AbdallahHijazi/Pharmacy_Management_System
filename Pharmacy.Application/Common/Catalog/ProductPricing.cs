using Pharmacy.Domain.Enums;
using Pharmacy.Domain.Exceptions;

namespace Pharmacy.Application.Common.Catalog
{
    /// <summary>منطق عرض الربح على مستوى بطاقة المنتج (بدون محرك ربح أو دفتر حركات).</summary>
    public static class ProductPricing
    {
        public static decimal? UnitProfit(decimal sellingPrice, decimal? referencePurchasePrice) =>
            referencePurchasePrice.HasValue ? sellingPrice - referencePurchasePrice.Value : null;

        public static void ValidateCatalogPurchase(ProductPricingType pricingType, decimal? purchasePrice)
        {
            if (pricingType == ProductPricingType.NationalMedicine && purchasePrice is null)
                throw new BadRequestException("سعر الشراء مطلوب للأدوية الوطنية");

            if (purchasePrice is < 0)
                throw new BadRequestException("سعر الشراء يجب أن يكون أكبر من أو يساوي صفر");
        }
    }
}
