using MediatR;
using Pharmacy.Application.DTOs.Reports;

namespace Pharmacy.Application.Features.Reports.Queries.GetMonthlyFinancialReport
{
    public sealed class GetMonthlyFinancialReportQuery : IRequest<MonthlyFinancialReportDto>
    {
        public int Year { get; set; }
        public int Month { get; set; }
    }
}
