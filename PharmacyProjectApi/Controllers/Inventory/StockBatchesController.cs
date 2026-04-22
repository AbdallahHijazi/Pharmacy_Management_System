using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy.API.Infrastructure;
using Pharmacy.Application.DTOs.Inventory;
using Pharmacy.Application.Features.Inventory.Commands.CreateStockBatch;
using Pharmacy.Application.Features.Inventory.Commands.UpdateStockBatch;
using Pharmacy.Application.Features.Inventory.Queries.GetStockBatchById;
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

        [HttpPost]
        [ProducesResponseType(typeof(StockBatchDetailsDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateStockBatch([FromBody] CreateStockBatchRequestDto request)
        {
            var result = await _mediator.Send(new CreateStockBatchCommand
            {
                ProductId = request.ProductId,
                BatchNumber = request.BatchNumber,
                ExpiryDate = request.ExpiryDate,
                PurchasePrice = request.PurchasePrice,
                ReceivedQuantity = request.ReceivedQuantity,
                AvailableQuantity = request.AvailableQuantity,
                SupplierId = request.SupplierId
            });

            return CreatedAtAction(nameof(GetStockBatchById), new { id = result.StockBatchId }, result);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(StockBatchDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetStockBatchById(Guid id)
        {
            var result = await _mediator.Send(new GetStockBatchByIdQuery
            {
                StockBatchId = id
            });

            return Ok(result);
        }


        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(StockBatchDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateStockBatch(Guid id, [FromBody] UpdateStockBatchRequestDto request)
        {
            var result = await _mediator.Send(new UpdateStockBatchCommand
            {
                StockBatchId = id,
                ProductId = request.ProductId,
                BatchNumber = request.BatchNumber,
                ExpiryDate = request.ExpiryDate,
                PurchasePrice = request.PurchasePrice,
                ReceivedQuantity = request.ReceivedQuantity,
                AvailableQuantity = request.AvailableQuantity,
                SupplierId = request.SupplierId
            });

            return Ok(result);
        }
    }
}
