using Pharmacy.Domain.Entities.Base;
using Pharmacy.Domain.Entities.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Domain.Entities.Purchases
{
    public class PurchaseReturnItem : BaseEntity
    {
        public Guid PurchaseReturnId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public PurchaseReturn PurchaseReturn { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}
