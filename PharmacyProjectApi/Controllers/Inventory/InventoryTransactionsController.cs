using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy.API.Infrastructure;
using Pharmacy.Application.DTOs.Inventory;
using Pharmacy.Application.Features.Inventory.Queries.GetInventoryTransactions;

namespace PharmacyProjectApi.Controllers.Inventory
{
    [ApiController]
    [Authorize]
    [Route("api/v1/inventory-transactions")]
    public class InventoryTransactionsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public InventoryTransactionsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<InventoryTransactionListItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetInventoryTransactions()
        {
            var result = await _mediator.Send(new GetInventoryTransactionsQuery());
            return Ok(result);
        }
    }
}
