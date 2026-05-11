using Pharmacy.Domain.Entities.Base;
using Pharmacy.Domain.Entities.Organization;
using Pharmacy.Domain.Entities.Partners;
using Pharmacy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Domain.Entities.Catalog
{
    public class Product : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string ScientificName { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
        public decimal SellingPrice { get; set; }
        /// <summary>نوع التسعير على مستوى البطاقة؛ لا يعدّل سعر البيع تلقائيًا.</summary>
        public ProductPricingType PricingType { get; set; } = ProductPricingType.FreePricingMedicine;
        /// <summary>سعر شراء مرجعي للبطاقة (وطني: مطلوب لحساب ربح الوحدة؛ حر: اختياري).</summary>
        public decimal? ReferencePurchasePrice { get; set; }
        public Guid? DefaultSupplierId { get; set; }
        public Guid BranchId { get; set; }

        public ProductCategory Category { get; set; } = null!;
        public Supplier? DefaultSupplier { get; set; }
        public Branch Branch { get; set; } = null!;
        public ICollection<StockBatch> StockBatches { get; set; } = new List<StockBatch>();
    }
}
