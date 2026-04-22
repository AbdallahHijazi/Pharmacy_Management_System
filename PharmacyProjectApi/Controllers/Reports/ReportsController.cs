using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy.API.Infrastructure;
using Pharmacy.Application.DTOs.Reports;
using Pharmacy.Application.Features.Reports.Queries.GetSalesReport;

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
    }
}
