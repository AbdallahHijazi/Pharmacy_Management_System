using MediatR;
using Pharmacy.Application.DTOs.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Products.Commands.UpdateProduct
{
    public class UpdateProductCommand : IRequest<ProductDetailsDto>
    {
        public Guid ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ScientificName { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
        public decimal SellingPrice { get; set; }
        public Guid? DefaultSupplierId { get; set; }
    }
}
