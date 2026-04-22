using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy.API.Infrastructure;
using Pharmacy.Application.DTOs.Purchases;
using Pharmacy.Application.Features.Purchases.Commands.CreatePurchaseInvoice;
using Pharmacy.Application.Features.Purchases.Queries.GetPurchaseInvoiceById;
using Pharmacy.Application.Features.Purchases.Queries.GetPurchaseInvoiceItems;
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

        [HttpPost]
        [ProducesResponseType(typeof(PurchaseInvoiceDetailsDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreatePurchaseInvoice([FromBody] CreatePurchaseInvoiceRequestDto request)
        {
            var result = await _mediator.Send(new CreatePurchaseInvoiceCommand
            {
                InvoiceNumber = request.InvoiceNumber,
                SupplierId = request.SupplierId,
                TaxRate = request.TaxRate,
                PaidAmount = request.PaidAmount,
                PaymentMethod = request.PaymentMethod,
                Items = request.Items
            });

            return CreatedAtAction(nameof(GetPurchaseInvoiceById), new { id = result.PurchaseInvoiceId }, result);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(PurchaseInvoiceDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPurchaseInvoiceById(Guid id)
        {
            var result = await _mediator.Send(new GetPurchaseInvoiceByIdQuery
            {
                PurchaseInvoiceId = id
            });

            return Ok(result);
        }

        [HttpGet("{id:guid}/items")]
        [ProducesResponseType(typeof(List<PurchaseInvoiceItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetPurchaseInvoiceItems(Guid id)
        {
            var result = await _mediator.Send(new GetPurchaseInvoiceItemsQuery
            {
                PurchaseInvoiceId = id
            });

            return Ok(result);
        }
    }
}
