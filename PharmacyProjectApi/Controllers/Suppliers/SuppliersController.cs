using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy.API.Infrastructure;
using Pharmacy.Application.DTOs.Inventory;
using Pharmacy.Application.DTOs.Suppliers;
using Pharmacy.Application.Features.Inventory.Queries.GetStockBatchById;
using Pharmacy.Application.Features.Suppliers.Commands.CreateSupplier;
using Pharmacy.Application.Features.Suppliers.Commands.DeleteSupplier;
using Pharmacy.Application.Features.Suppliers.Commands.UpdateSupplier;
using Pharmacy.Application.Features.Suppliers.Queries.GetSupplierById;
using Pharmacy.Application.Features.Suppliers.Queries.GetSuppliers;

namespace PharmacyProjectApi.Controllers.Suppliers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/suppliers")]
    public class SuppliersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SuppliersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<SupplierListItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetSuppliers()
        {
            var result = await _mediator.Send(new GetSuppliersQuery());
            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(SupplierDetailsDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateSupplier([FromBody] CreateSupplierRequestDto request)
        {
            var result = await _mediator.Send(new CreateSupplierCommand
            {
                Name = request.Name,
                ContactPerson = request.ContactPerson,
                Phone = request.Phone,
                Address = request.Address
            });

            return CreatedAtAction(nameof(GetSupplierById), new { id = result.SupplierId }, result);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(SupplierDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSupplierById(Guid id)
        {
            var result = await _mediator.Send(new GetSupplierByIdQuery
            {
                SupplierId = id
            });

            return Ok(result);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(SupplierDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateSupplier(Guid id, [FromBody] UpdateSupplierRequestDto request)
        {
            var result = await _mediator.Send(new UpdateSupplierCommand
            {
                SupplierId = id,
                Name = request.Name,
                ContactPerson = request.ContactPerson,
                Phone = request.Phone,
                Address = request.Address
            });

            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteSupplier(Guid id)
        {
            await _mediator.Send(new DeleteSupplierCommand
            {
                SupplierId = id
            });

            return Ok(new { message = "تم حذف المورد بنجاح" });
        }


    }
}
