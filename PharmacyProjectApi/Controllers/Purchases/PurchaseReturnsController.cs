using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy.API.Infrastructure;
using Pharmacy.Application.DTOs.Purchases;
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
    }
}
