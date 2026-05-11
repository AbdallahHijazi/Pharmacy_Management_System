using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Domain.Enums
{
    public enum ReferenceType { 
        SalesInvoice, 
        PurchaseInvoice, 
        SalesReturn, 
        PurchaseReturn,
        /// <summary>Manual batch creation (no purchase invoice line).</summary>
        StockBatchManualEntry,
        /// <summary>Direct stock adjustment or batch master-data correction tied to a batch.</summary>
        StockBatchAdjustment
    }
}
