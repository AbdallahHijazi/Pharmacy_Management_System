using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy.API.Infrastructure;
using Pharmacy.Application.DTOs.Purchases;
using Pharmacy.Application.Features.Purchases.Commands.CreatePurchaseReturn;
using Pharmacy.Application.Features.Purchases.Queries.GetPurchaseReturnById;
using Pharmacy.Application.Features.Purchases.Queries.GetPurchaseReturns;

namespace PharmacyProjectApi.Controllers.Purchases
{
    [ApiController]
    [Authorize]
    [Route("api/v1/purchase-returns")]
    public class PurchaseReturnsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PurchaseReturnsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<PurchaseReturnListItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetPurchaseReturns()
        {
            var result = await _mediator.Send(new GetPurchaseReturnsQuery());
            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(PurchaseReturnDetailsDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreatePurchaseReturn([FromBody] CreatePurchaseReturnRequestDto request)
        {
            var result = await _mediator.Send(new CreatePurchaseReturnCommand
            {
                PurchaseInvoiceId = request.PurchaseInvoiceId,
                Reason = request.Reason,
                Items = request.Items
            });

            return CreatedAtAction(nameof(GetPurchaseReturnById), new { id = result.PurchaseReturnId }, result);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(PurchaseReturnDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPurchaseReturnById(Guid id)
        {
            var result = await _mediator.Send(new GetPurchaseReturnByIdQuery
            {
                PurchaseReturnId = id
            });

            return Ok(result);
        }
    }
}
