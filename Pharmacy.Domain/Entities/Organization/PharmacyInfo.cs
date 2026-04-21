using Pharmacy.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Domain.Entities.Organization
{
    public class PharmacyInfo : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Currency { get; set; } = "SYP";
        public decimal ExchangeRate { get; set; } = 1m;
        public decimal TaxRate { get; set; } = 0m;

        public ICollection<Branch> Branches { get; set; } = new List<Branch>();
    }
}
