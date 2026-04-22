using MediatR;
using Pharmacy.Application.DTOs.Dashboard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Dashboard.Queries.GetTopSellingProducts
{
    public class GetTopSellingProductsQuery : IRequest<List<TopSellingProductDto>>
    {
    }
}
