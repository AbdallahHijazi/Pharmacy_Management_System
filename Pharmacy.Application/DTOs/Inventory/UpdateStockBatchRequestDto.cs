using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.DTOs.Inventory
{
    /// <summary>
    /// تحديث بيانات الدفعة. تعديل <see cref="ReceivedQuantity"/> غير مسموح إن وُجدت حركات مخزون أو بنود مبيعات على الدفعة؛ استخدم تعديل المخزون بدلًا من ذلك.
    /// </summary>
    public class UpdateStockBatchRequestDto
    {
        public Guid ProductId { get; set; }
        public string LotNumber { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public decimal PurchasePrice { get; set; }
        public int ReceivedQuantity { get; set; }
        public Guid SupplierId { get; set; }
    }
}
