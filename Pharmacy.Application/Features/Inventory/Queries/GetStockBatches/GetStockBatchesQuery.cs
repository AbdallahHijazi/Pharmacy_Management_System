using MediatR;
using Pharmacy.Application.Common.Models;
using Pharmacy.Application.DTOs.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Inventory.Queries.GetStockBatches
{
    public class GetStockBatchesQuery : IRequest<PagedResult<StockBatchListItemDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; } = "expirydate";
        public string? SortDirection { get; set; } = "asc";
    }
}
