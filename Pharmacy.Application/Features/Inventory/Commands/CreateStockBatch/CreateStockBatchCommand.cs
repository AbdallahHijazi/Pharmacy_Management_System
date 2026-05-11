using MediatR;
using Pharmacy.Application.DTOs.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Inventory.Commands.CreateStockBatch
{
    public class CreateStockBatchCommand : IRequest<StockBatchDetailsDto>
    {
        public Guid ProductId { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public decimal PurchasePrice { get; set; }
        public int ReceivedQuantity { get; set; }
        public Guid SupplierId { get; set; }
    }
}
