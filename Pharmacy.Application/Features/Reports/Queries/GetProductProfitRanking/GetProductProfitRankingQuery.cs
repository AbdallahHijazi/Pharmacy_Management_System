using MediatR;
using Pharmacy.Application.DTOs.Reports;

namespace Pharmacy.Application.Features.Reports.Queries.GetProductProfitRanking
{
    public sealed class GetProductProfitRankingQuery : IRequest<ProductProfitRankingReportDto>
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        /// <summary>BestProfit أو WorstProfit (غير حساس لحالة الأحرف).</summary>
        public string Rank { get; set; } = "BestProfit";
        public int Take { get; set; } = 10;
    }
}
