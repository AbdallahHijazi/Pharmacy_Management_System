using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Domain.Enums
{
    public enum SalesInvoiceStatus { 
        Pending, 
        Completed, 
        PartiallyPaid, 
        Returned, 
        Cancelled 
    }
}
