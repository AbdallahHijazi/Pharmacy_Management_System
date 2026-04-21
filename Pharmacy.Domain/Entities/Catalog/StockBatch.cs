using Pharmacy.Domain.Entities.Base;
using Pharmacy.Domain.Entities.Inventory;
using Pharmacy.Domain.Entities.Organization;
using Pharmacy.Domain.Entities.Partners;
using Pharmacy.Domain.Entities.Sales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Domain.Entities.Catalog
{
    public class StockBatch : BaseEntity
    {
        public Guid ProductId { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public decimal PurchasePrice { get; set; }
        public int ReceivedQuantity { get; set; }
        public int AvailableQuantity { get; set; }
        public Guid SupplierId { get; set; }
        public Guid BranchId { get; set; }

        public Product Product { get; set; } = null!;
        public Supplier Supplier { get; set; } = null!;
        public Branch Branch { get; set; } = null!;
        public ICollection<InventoryTransaction> Transactions { get; set; } =new List<InventoryTransaction>();
        public ICollection<SalesInvoiceItem> SalesItems { get; set; } = new List<SalesInvoiceItem>();
    }
}
