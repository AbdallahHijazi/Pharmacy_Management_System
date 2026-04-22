using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.DTOs.Dashboard
{
    public class DashboardStatsDto
    {
        public int TotalProducts { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalSuppliers { get; set; }
        public int TodayInvoicesCount { get; set; }
        public decimal TodaySalesTotal { get; set; }
        public int LowStockProductsCount { get; set; }
        public int ExpiringSoonBatchesCount { get; set; }
    }
}
