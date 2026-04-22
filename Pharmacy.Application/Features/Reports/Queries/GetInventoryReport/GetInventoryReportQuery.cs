using MediatR;
using Pharmacy.Application.DTOs.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Reports.Queries.GetInventoryReport
{
    public class GetInventoryReportQuery : IRequest<InventoryReportDto>
    {
    }
}
