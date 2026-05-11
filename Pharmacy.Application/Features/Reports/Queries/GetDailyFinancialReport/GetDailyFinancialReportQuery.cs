using MediatR;
using Pharmacy.Application.DTOs.Reports;

namespace Pharmacy.Application.Features.Reports.Queries.GetDailyFinancialReport
{
    public sealed class GetDailyFinancialReportQuery : IRequest<DailyFinancialReportDto>
    {
        public DateTime Date { get; set; }
    }
}
