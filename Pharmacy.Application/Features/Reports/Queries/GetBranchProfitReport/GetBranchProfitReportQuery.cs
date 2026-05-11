using MediatR;
using Pharmacy.Application.DTOs.Reports;

namespace Pharmacy.Application.Features.Reports.Queries.GetBranchProfitReport
{
    public sealed class GetBranchProfitReportQuery : IRequest<BranchProfitReportDto>
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}
