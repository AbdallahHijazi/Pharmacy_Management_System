using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy.API.Infrastructure;
using Pharmacy.Application.DTOs.Settings;
using Pharmacy.Application.Features.Settings.Commands.UpdateSetting;
using Pharmacy.Application.Features.Settings.Queries.GetSettings;

namespace PharmacyProjectApi.Controllers.Settings
{
    [ApiController]
    [Authorize]
    [Route("api/v1/settings")]
    public class SettingsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SettingsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(SystemSettingDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSettings()
        {
            var result = await _mediator.Send(new GetSettingsQuery());
            return Ok(result);
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UpdateSetting([FromBody] UpdateSystemSettingRequestDto request)
        {
            await _mediator.Send(new UpdateSettingCommand
            {
                SettingId = request.SettingId,
                Value = request.Value
            });

            return NoContent();
        }
    }
}
