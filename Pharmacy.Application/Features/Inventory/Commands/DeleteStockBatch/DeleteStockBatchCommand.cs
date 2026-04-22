using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Inventory.Commands.DeleteStockBatch
{
    public class DeleteStockBatchCommand : IRequest<Unit>
    {
        public Guid StockBatchId { get; set; }
    }
}
