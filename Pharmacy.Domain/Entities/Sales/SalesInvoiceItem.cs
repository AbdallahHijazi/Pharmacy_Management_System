using Pharmacy.Domain.Entities.Base;
using Pharmacy.Domain.Entities.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Domain.Entities.Sales
{
    public class SalesInvoiceItem : BaseEntity
    {
        public Guid SalesInvoiceId { get; set; }
        public Guid StockBatchId { get; set; }
        public int Quantity { get; set; }
        /// <summary>سعر بيع الوحدة وقت إصدار السطر (من بطاقة المنتج وقت البيع).</summary>
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }

        /// <summary>تكلفة الوحدة الفعّالة وقت البيع (COGS basis) — null للبيانات القديمة قبل التتبع.</summary>
        public decimal? UnitEffectiveCostAtSale { get; set; }
        /// <summary>نسخة سعر شراء الوحدة على الدفعة وقت البيع.</summary>
        public decimal? BatchNominalPurchasePriceAtSale { get; set; }
        public int? BatchReceivedQuantityAtSale { get; set; }
        public int? BatchBonusQuantityAtSale { get; set; }

        public SalesInvoice SalesInvoice { get; set; } = null!;
        public StockBatch StockBatch { get; set; } = null!;
    }
}
