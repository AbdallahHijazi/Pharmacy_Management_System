using Pharmacy.Domain.Entities.Base;
using Pharmacy.Domain.Entities.Catalog;
using Pharmacy.Domain.Entities.Identity;
using Pharmacy.Domain.Entities.Organization;
using Pharmacy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Domain.Entities.Inventory
{
    public class InventoryTransaction : BaseEntity
    {
        /// <summary>Optional denormalized product for audit (e.g. manual batch creation). Falls back to <see cref="StockBatch"/> when null.</summary>
        public Guid? ProductId { get; set; }

        public Guid StockBatchId { get; set; }
        public TransactionType Type { get; set; }
        public int Quantity { get; set; }
        public string Reason { get; set; } = string.Empty;
        public Guid? ReferenceId { get; set; }
        public ReferenceType ReferenceType { get; set; }
        public Guid UserId { get; set; }
        public Guid BranchId { get; set; }

        public StockBatch StockBatch { get; set; } = null!;
        public User User { get; set; } = null!;
        public Branch Branch { get; set; } = null!;
    }
}
