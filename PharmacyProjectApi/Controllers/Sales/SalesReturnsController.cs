using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy.API.Infrastructure;
using Pharmacy.Application.DTOs.Sales;
using Pharmacy.Application.Features.Sales.Queries.GetSalesReturns;

namespace PharmacyProjectApi.Controllers.Sales
{
    [ApiController]
    [Authorize]
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
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetSalesReturns()
        {
            var result = await _mediator.Send(new GetSalesReturnsQuery());
            return Ok(result);
        }
    }
}
