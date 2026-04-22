using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy.API.Infrastructure;
using Pharmacy.Application.DTOs.Roles;
using Pharmacy.Application.Features.Roles.Commands.CreateRole;
using Pharmacy.Application.Features.Roles.Commands.UpdateRole;
using Pharmacy.Application.Features.Roles.Queries.GetRoleById;
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

        [HttpPost]
        [ProducesResponseType(typeof(RoleDetailsDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequestDto request)
        {
            var command = new CreateRoleCommand
            {
                Name = request.Name,
                Description = request.Description
            };

            var result = await _mediator.Send(command);

            return CreatedAtAction(nameof(GetRoleById), new { id = result.RoleId }, result);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(RoleDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRoleById(Guid id)
        {
            var result = await _mediator.Send(new GetRoleByIdQuery
            {
                RoleId = id
            });

            return Ok(result);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(RoleDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateRoleRequestDto request)
        {
            var command = new UpdateRoleCommand
            {
                RoleId = id,
                Name = request.Name,
                Description = request.Description
            };

            var result = await _mediator.Send(command);

            return Ok(result);
        }
    }
}
