using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy.API.Infrastructure;
using Pharmacy.Application.DTOs.Reports;
using Pharmacy.Application.Features.Reports.Queries.GetInventoryReport;
using Pharmacy.Application.Features.Reports.Queries.GetPurchasesReport;
using Pharmacy.Application.Features.Reports.Queries.GetSalesReport;
using Pharmacy.Application.Features.Reports.Queries.GetStockConsistencyDiagnostics;

namespace PharmacyProjectApi.Controllers.Reports
{
    [ApiController]
    [Authorize]
    [Route("api/v1/reports")]
    public class ReportsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReportsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("sales")]
        [ProducesResponseType(typeof(SalesReportDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetSalesReport(
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate,
            [FromQuery] Guid? customerId)
        {
            var result = await _mediator.Send(new GetSalesReportQuery
            {
                FromDate = fromDate,
                ToDate = toDate,
                CustomerId = customerId
            });

            return Ok(result);
        }

        [HttpGet("purchases")]
        [ProducesResponseType(typeof(PurchasesReportDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetPurchasesReport(
                    [FromQuery] DateTime fromDate,
                    [FromQuery] DateTime toDate,
                    [FromQuery] Guid? supplierId)
        {
            var result = await _mediator.Send(new GetPurchasesReportQuery
            {
                FromDate = fromDate,
                ToDate = toDate,
                SupplierId = supplierId
            });

            return Ok(result);
        }

        [HttpGet("inventory")]
        [ProducesResponseType(typeof(InventoryReportDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetInventoryReport()
        {
            var result = await _mediator.Send(new GetInventoryReportQuery());
            return Ok(result);
        }

        [HttpGet("inventory/stock-consistency-diagnostics")]
        [ProducesResponseType(typeof(StockConsistencyDiagnosticsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetStockConsistencyDiagnostics()
        {
            var result = await _mediator.Send(new GetStockConsistencyDiagnosticsQuery());
            return Ok(result);
        }
    }
}
