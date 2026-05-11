namespace Pharmacy.Domain.Enums
{
    /// <summary>تصنيف تسعير المنتج (بطاقة الصنف) — لا يغيّر سعر البيع تلقائيًا.</summary>
    public enum ProductPricingType
    {
        /// <summary>دواء تسعير حر — المستخدم يتحكم بسعر البيع؛ سعر الشراء المرجعي اختياري لاحقًا للربح.</summary>
        FreePricingMedicine = 0,

        /// <summary>دواء وطني — مرجع سعر البيع (وزارة) يُدخل يدويًا مع سعر الشراء؛ الربح يُشتق للعرض فقط.</summary>
        NationalMedicine = 1
    }
}
