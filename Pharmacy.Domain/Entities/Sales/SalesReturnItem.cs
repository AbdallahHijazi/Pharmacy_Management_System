using Pharmacy.Domain.Entities.Base;
using Pharmacy.Domain.Entities.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Domain.Entities.Sales
{
    public class SalesReturnItem : BaseEntity
    {
        public Guid SalesReturnId { get; set; }
        /// <summary>سطر فاتورة البيع الأصلي (لتتبع تكلفة المرتجع بدقة).</summary>
        public Guid? SalesInvoiceItemId { get; set; }
        public Guid StockBatchId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public SalesReturn SalesReturn { get; set; } = null!;
        public SalesInvoiceItem? SalesInvoiceItem { get; set; }
        public StockBatch StockBatch { get; set; } = null!;
    }
}
