using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy.API.Infrastructure;
using Pharmacy.Application.DTOs.Roles;
using Pharmacy.Application.Features.Roles.Queries.GetRoles;

namespace PharmacyProjectApi.Controllers.Roles
{
    [ApiController]
    [Authorize]
    [Route("api/v1/roles")]
    public class RolesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RolesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<RoleListItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetRoles()
        {
            var result = await _mediator.Send(new GetRolesQuery());
            return Ok(result);
        }
    }
}
