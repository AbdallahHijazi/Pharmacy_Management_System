using MediatR;
using Pharmacy.Application.DTOs.Suppliers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Suppliers.Queries.GetSupplierById
{
    public class GetSupplierByIdQuery : IRequest<SupplierDetailsDto>
    {
        public Guid SupplierId { get; set; }
    }
}
