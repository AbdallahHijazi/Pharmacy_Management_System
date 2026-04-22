using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy.API.Infrastructure;
using Pharmacy.Application.DTOs.Permissions;
using Pharmacy.Application.DTOs.Roles;
using Pharmacy.Application.Features.Permissions.Queries.GetPermissions;
using Pharmacy.Application.Features.Roles.Commands.AssignPermissionsToRole;
using Pharmacy.Application.Features.Roles.Queries.GetRolePermissions;

namespace PharmacyProjectApi.Controllers.Permissions
{
    [ApiController]
    [Authorize]
    [Route("api/v1/permissions")]
    public class PermissionsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PermissionsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<PermissionListItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetPermissions()
        {
            var result = await _mediator.Send(new GetPermissionsQuery());
            return Ok(result);
        }

        [HttpPost("{id:guid}/permissions")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AssignPermissionsToRole(Guid id, [FromBody] AssignPermissionsToRoleRequestDto request)
        {
            await _mediator.Send(new AssignPermissionsToRoleCommand
            {
                RoleId = id,
                PermissionIds = request.PermissionIds
            });

            return Ok(new { message = "تم تحديث صلاحيات الدور بنجاح" });
        }

        [HttpGet("{id:guid}/permissions")]
        [ProducesResponseType(typeof(List<RolePermissionItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRolePermissions(Guid id)
        {
            var result = await _mediator.Send(new GetRolePermissionsQuery
            {
                RoleId = id
            });

            return Ok(result);
        }
    }
}
