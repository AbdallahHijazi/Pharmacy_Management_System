using Pharmacy.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Domain.Entities.Catalog
{
    public class ProductCategory : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
    }
}
