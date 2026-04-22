using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy.API.Infrastructure;
using Pharmacy.Application.DTOs.Customers;
using Pharmacy.Application.Features.Customers.Commands.CreateCustomer;
using Pharmacy.Application.Features.Customers.Commands.DeleteCustomer;
using Pharmacy.Application.Features.Customers.Commands.UpdateCustomer;
using Pharmacy.Application.Features.Customers.Queries.GetCustomerById;
using Pharmacy.Application.Features.Customers.Queries.GetCustomers;
using Pharmacy.Application.Features.Customers.Queries.SearchCustomers;

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

            return CreatedAtAction(nameof(GetCustomerById), new { id = result.CustomerId }, result);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(CustomerDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCustomerById(Guid id)
        {
            var result = await _mediator.Send(new GetCustomerByIdQuery
            {
                CustomerId = id
            });

            return Ok(result);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(CustomerDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCustomer(Guid id, [FromBody] UpdateCustomerRequestDto request)
        {
            var result = await _mediator.Send(new UpdateCustomerCommand
            {
                CustomerId = id,
                FullName = request.FullName,
                Phone = request.Phone,
                Address = request.Address
            });

            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCustomer(Guid id)
        {
            await _mediator.Send(new DeleteCustomerCommand
            {
                CustomerId = id
            });

            return Ok(new { message = "تم حذف الزبون بنجاح" });
        }

        [HttpGet("search")]
        [ProducesResponseType(typeof(List<CustomerListItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SearchCustomers([FromQuery] string query)
        {
            var result = await _mediator.Send(new SearchCustomersQuery
            {
                Query = query
            });

            return Ok(result);
        }
    }
}
