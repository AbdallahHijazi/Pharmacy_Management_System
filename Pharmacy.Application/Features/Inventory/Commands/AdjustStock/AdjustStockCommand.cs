using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Inventory.Commands.AdjustStock
{
    public class AdjustStockCommand : IRequest<Unit>
    {
        public Guid StockBatchId { get; set; }
        public string Type { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
