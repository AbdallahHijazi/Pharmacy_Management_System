using MediatR;
using Pharmacy.Application.DTOs.Reports;

namespace Pharmacy.Application.Features.Reports.Queries.GetStockConsistencyDiagnostics
{
    public sealed class GetStockConsistencyDiagnosticsQuery : IRequest<StockConsistencyDiagnosticsDto>
    {
    }
}
