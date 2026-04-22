using MediatR;
using Pharmacy.Application.DTOs.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Inventory.Queries.GetLowStockBatches
{
    public class GetLowStockBatchesQuery : IRequest<List<StockBatchListItemDto>>
    {
    }
}
