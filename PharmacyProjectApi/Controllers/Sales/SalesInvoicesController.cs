using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy.API.Infrastructure;
using Pharmacy.Application.DTOs.Sales;
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
    }
}
