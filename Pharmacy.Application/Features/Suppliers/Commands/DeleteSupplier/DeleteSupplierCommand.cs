using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Suppliers.Commands.DeleteSupplier
{
    public class DeleteSupplierCommand : IRequest<Unit>
    {
        public Guid SupplierId { get; set; }
    }
}
