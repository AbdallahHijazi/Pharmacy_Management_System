using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy.API.Infrastructure;
using Pharmacy.Application.DTOs.Sales;
using Pharmacy.Application.Features.Sales.Commands.CreateSalesInvoice;
using Pharmacy.Application.Features.Sales.Queries.GetSalesInvoiceById;
using Pharmacy.Application.Features.Sales.Queries.GetSalesInvoices;

namespace PharmacyProjectApi.Controllers.Sales
{
    [ApiController]
    [Authorize]
    [Route("api/v1/sales-invoices")]
    public class SalesInvoicesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SalesInvoicesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<SalesInvoiceListItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetSalesInvoices()
        {
            var result = await _mediator.Send(new GetSalesInvoicesQuery());
            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(SalesInvoiceDetailsDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateSalesInvoice([FromBody] CreateSalesInvoiceRequestDto request)
        {
            var result = await _mediator.Send(new CreateSalesInvoiceCommand
            {
                CustomerId = request.CustomerId,
                DiscountPercentage = request.DiscountPercentage,
                PaidAmount = request.PaidAmount,
                PaymentMethod = request.PaymentMethod,
                Items = request.Items
            });

            return CreatedAtAction(nameof(GetSalesInvoiceById), new { id = result.SalesInvoiceId }, result);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(SalesInvoiceDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetSalesInvoiceById(Guid id)
        {
            var result = await _mediator.Send(new GetSalesInvoiceByIdQuery
            {
                SalesInvoiceId = id
            });

            return Ok(result);
        }
    }
}
