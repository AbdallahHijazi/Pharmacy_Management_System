using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy.API.Infrastructure;
using Pharmacy.Application.DTOs.Dashboard;
using Pharmacy.Application.Features.Dashboard.Queries.GetDashboardStats;
using Pharmacy.Application.Features.Dashboard.Queries.GetExpiringSoonBatchesDashboard;
using Pharmacy.Application.Features.Dashboard.Queries.GetLatestSalesInvoices;
using Pharmacy.Application.Features.Dashboard.Queries.GetLowStockProducts;
using Pharmacy.Application.Features.Dashboard.Queries.GetTopSellingProducts;

namespace PharmacyProjectApi.Controllers.Dashboard
{
    [ApiController]
    [Authorize]
    [Route("api/v1/dashboard")]
    public class DashboardController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("stats")]
        [ProducesResponseType(typeof(DashboardStatsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetDashboardStats()
        {
            var result = await _mediator.Send(new GetDashboardStatsQuery());
            return Ok(result);
        }

        [HttpGet("low-stock-products")]
        [ProducesResponseType(typeof(List<LowStockProductDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetLowStockProducts()
        {
            var result = await _mediator.Send(new GetLowStockProductsQuery());
            return Ok(result);
        }

        [HttpGet("expiring-soon-batches")]
        [ProducesResponseType(typeof(List<ExpiringSoonBatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetExpiringSoonBatches()
        {
            var result = await _mediator.Send(new GetExpiringSoonBatchesDashboardQuery());
            return Ok(result);
        }

        [HttpGet("latest-sales-invoices")]
        [ProducesResponseType(typeof(List<LatestSalesInvoiceDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetLatestSalesInvoices()
        {
            var result = await _mediator.Send(new GetLatestSalesInvoicesQuery());
            return Ok(result);
        }

        [HttpGet("top-selling-products")]
        [ProducesResponseType(typeof(List<TopSellingProductDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetTopSellingProducts()
        {
            var result = await _mediator.Send(new GetTopSellingProductsQuery());
            return Ok(result);
        }
    }
}
