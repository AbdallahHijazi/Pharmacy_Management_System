using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy.API.Infrastructure;
using Pharmacy.Application.DTOs.Sales;
using Pharmacy.Application.Features.Sales.Commands.CreateSalesReturn;
using Pharmacy.Application.Features.Sales.Queries.GetSalesReturnById;
using Pharmacy.Application.Features.Sales.Queries.GetSalesReturnItems;
using Pharmacy.Application.Features.Sales.Queries.GetSalesReturns;

namespace PharmacyProjectApi.Controllers.Sales
{
    [ApiController]
    //[Authorize]
    [Route("api/v1/sales-returns")]
    public class SalesReturnsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SalesReturnsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<SalesReturnListItemDto>), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetSalesReturns()
        {
            var result = await _mediator.Send(new GetSalesReturnsQuery());
            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(SalesReturnDetailsDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateSalesReturn([FromBody] CreateSalesReturnRequestDto request)
        {
            var result = await _mediator.Send(new CreateSalesReturnCommand
            {
                SalesInvoiceId = request.SalesInvoiceId,
                Reason = request.Reason,
                Items = request.Items
            });

            return CreatedAtAction(nameof(GetSalesReturnById), new { id = result.SalesReturnId }, result);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(SalesReturnDetailsDto), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSalesReturnById(Guid id)
        {
            var result = await _mediator.Send(new GetSalesReturnByIdQuery
            {
                SalesReturnId = id
            });

            return Ok(result);
        }

        [HttpGet("{id:guid}/items")]
        [ProducesResponseType(typeof(List<SalesReturnItemDto>), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetSalesReturnItems(Guid id)
        {
            var result = await _mediator.Send(new GetSalesReturnItemsQuery
            {
                SalesReturnId = id
            });

            return Ok(result);
        }
    }
}
