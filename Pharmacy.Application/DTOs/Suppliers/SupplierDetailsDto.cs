using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.DTOs.Suppliers
{
    public class SupplierDetailsDto
    {
        public Guid SupplierId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public decimal TotalPurchases { get; set; }
        public decimal PayableAmount { get; set; }
        public Guid BranchId { get; set; }
    }
}
