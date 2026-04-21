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
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }

        public SalesInvoice SalesInvoice { get; set; } = null!;
        public StockBatch StockBatch { get; set; } = null!;
    }
}
