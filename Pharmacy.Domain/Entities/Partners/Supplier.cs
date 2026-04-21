using Pharmacy.Domain.Entities.Base;
using Pharmacy.Domain.Entities.Organization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Domain.Entities.Partners
{
    public class Supplier : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public decimal TotalPurchases { get; set; } = 0m;
        public decimal PayableAmount { get; set; } = 0m;
        public Guid BranchId { get; set; }

        public Branch Branch { get; set; } = null!;
    }
}
