using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.DTOs.Products
{
    public class CreateProductRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public string ScientificName { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
        public decimal SellingPrice { get; set; }
        public Guid? DefaultSupplierId { get; set; }
    }
}
