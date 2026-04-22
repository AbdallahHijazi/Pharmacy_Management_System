using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy.API.Infrastructure;
using Pharmacy.Application.DTOs.Inventory;
using Pharmacy.Application.Features.Inventory.Queries.GetStockBatches;

namespace PharmacyProjectApi.Controllers.Inventory
{
    [ApiController]
    [Authorize]
    [Route("api/v1/stock-batches")]
    public class StockBatchesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public StockBatchesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<StockBatchListItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetStockBatches()
        {
            var result = await _mediator.Send(new GetStockBatchesQuery());
            return Ok(result);
        }
    }
}
