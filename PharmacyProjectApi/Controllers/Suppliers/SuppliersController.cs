using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy.API.Infrastructure;
using Pharmacy.Application.DTOs.Suppliers;
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
    }
}
