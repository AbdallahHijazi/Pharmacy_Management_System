using MediatR;
using Pharmacy.Application.DTOs.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Reports.Queries.GetPurchasesReport
{
    public class GetPurchasesReportQuery : IRequest<PurchasesReportDto>
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public Guid? SupplierId { get; set; }
    }
}
