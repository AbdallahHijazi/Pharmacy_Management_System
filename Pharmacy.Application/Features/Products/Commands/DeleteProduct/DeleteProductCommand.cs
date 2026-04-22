using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Products.Commands.DeleteProduct
{
    public class DeleteProductCommand : IRequest<Unit>
    {
        public Guid ProductId { get; set; }
    }
}
