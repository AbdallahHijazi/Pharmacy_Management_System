using MediatR;
using Pharmacy.Application.DTOs.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Reports.Queries.GetSalesReport
{
    public class GetSalesReportQuery : IRequest<SalesReportDto>
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public Guid? CustomerId { get; set; }
    }
}
