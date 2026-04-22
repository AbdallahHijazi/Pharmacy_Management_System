using MediatR;
using Pharmacy.Application.DTOs.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Products.Queries.GetProducts
{
    public class GetProductsQuery : IRequest<List<ProductListItemDto>>
    {
    }
}
