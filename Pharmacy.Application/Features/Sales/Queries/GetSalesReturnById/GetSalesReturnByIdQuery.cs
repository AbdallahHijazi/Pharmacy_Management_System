using MediatR;
using Pharmacy.Application.DTOs.Sales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Sales.Queries.GetSalesReturnById
{
    public class GetSalesReturnByIdQuery : IRequest<SalesReturnDetailsDto>
    {
        public Guid SalesReturnId { get; set; }
    }
}
