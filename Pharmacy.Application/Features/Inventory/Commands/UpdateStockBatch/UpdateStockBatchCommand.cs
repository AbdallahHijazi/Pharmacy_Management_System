using MediatR;
using Pharmacy.Application.DTOs.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Inventory.Commands.UpdateStockBatch
{
    /// <summary>تحديث دفعة مخزون؛ قيود الكمية المستلمة مذكورة في <c>UpdateStockBatchRequestDto</c>.</summary>
    public class UpdateStockBatchCommand : IRequest<StockBatchDetailsDto>
    {
        public Guid StockBatchId { get; set; }
        public Guid ProductId { get; set; }
        public string LotNumber { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public decimal PurchasePrice { get; set; }
        public int ReceivedQuantity { get; set; }
        public int? BonusQuantity { get; set; }
        public Guid SupplierId { get; set; }
    }
}
