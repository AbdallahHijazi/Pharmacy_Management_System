using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy.API.Infrastructure;
using Pharmacy.Application.DTOs.Customers;
using Pharmacy.Application.Features.Customers.Queries.GetCustomers;

namespace PharmacyProjectApi.Controllers.Customers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/customers")]
    public class CustomersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CustomersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<CustomerListItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCustomers()
        {
            var result = await _mediator.Send(new GetCustomersQuery());
            return Ok(result);
        }
    }
}
