using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy.API.Infrastructure;
using Pharmacy.Application.DTOs.Notifications;
using Pharmacy.Application.Features.Notifications.Queries.GetNotifications;

namespace PharmacyProjectApi.Controllers.Notifications
{
    [ApiController]
    [Authorize]
    [Route("api/v1/notifications")]
    public class NotificationsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public NotificationsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<NotificationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetNotifications()
        {
            var result = await _mediator.Send(new GetNotificationsQuery());
            return Ok(result);
        }
    }
}
