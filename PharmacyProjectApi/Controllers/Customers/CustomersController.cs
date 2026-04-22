using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy.API.Infrastructure;
using Pharmacy.Application.DTOs.Customers;
using Pharmacy.Application.Features.Customers.Commands.CreateCustomer;
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

        [HttpPost]
        [ProducesResponseType(typeof(CustomerDetailsDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerRequestDto request)
        {
            var result = await _mediator.Send(new CreateCustomerCommand
            {
                FullName = request.FullName,
                Phone = request.Phone,
                Address = request.Address
            });

            return CreatedAtAction(nameof(GetCustomers), new { id = result.CustomerId }, result);
        }
    }
}
