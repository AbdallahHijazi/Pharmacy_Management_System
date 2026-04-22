using MediatR;
using Pharmacy.Application.DTOs.Purchases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Purchases.Queries.GetPurchaseReturnById
{
    public class GetPurchaseReturnByIdQuery : IRequest<PurchaseReturnDetailsDto>
    {
        public Guid PurchaseReturnId { get; set; }
    }
}
