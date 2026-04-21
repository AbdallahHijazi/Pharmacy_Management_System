using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pharmacy.API.Infrastructure;
using Pharmacy.Application.DTOs.Auth;
using Pharmacy.Application.Features.Auth.Commands.Login;

namespace PharmacyProjectApi.Controllers.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var command = new LoginCommand
            {
                Email = request.Email,
                Password = request.Password
            };

            var result = await _mediator.Send(command);

            return Ok(result);
        }
    }
}
