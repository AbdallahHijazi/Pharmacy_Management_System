using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy.API.Infrastructure;
using Pharmacy.Application.DTOs.Purchases;
using Pharmacy.Application.Features.Purchases.Queries.GetPurchaseInvoices;

namespace PharmacyProjectApi.Controllers.Purchases
{
    [ApiController]
    [Authorize]
    [Route("api/v1/purchase-invoices")]
    public class PurchaseInvoicesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PurchaseInvoicesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<PurchaseInvoiceListItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetPurchaseInvoices()
        {
            var result = await _mediator.Send(new GetPurchaseInvoicesQuery());
            return Ok(result);
        }
    }
}
