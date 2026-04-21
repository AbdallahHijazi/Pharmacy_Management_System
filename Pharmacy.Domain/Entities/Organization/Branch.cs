using Pharmacy.Domain.Entities.Base;
using Pharmacy.Domain.Entities.Catalog;
using Pharmacy.Domain.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Domain.Entities.Organization
{
    public class Branch : BaseEntity
    {
        public Guid PharmacyId { get; set; }
        public string Name { get; set; } = "الفرع الرئيسي";
        public string Address { get; set; } = string.Empty;

        public PharmacyInfo Pharmacy { get; set; } = null!;
        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<Product> Products { get; set; } = new List<Product>();
        public ICollection<StockBatch> StockBatches { get; set; } = new List<StockBatch>();
    }
}
