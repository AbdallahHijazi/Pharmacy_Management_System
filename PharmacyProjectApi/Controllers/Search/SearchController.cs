using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy.API.Infrastructure;
using Pharmacy.Application.DTOs.Search;
using Pharmacy.Application.Features.Search.Queries.GlobalSearch;

namespace PharmacyProjectApi.Controllers.Search
{
    [ApiController]
    [Authorize]
    [Route("api/v1/search")]
    public class SearchController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SearchController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<GlobalSearchResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GlobalSearch([FromQuery] string query)
        {
            var result = await _mediator.Send(new GlobalSearchQuery
            {
                Query = query
            });

            return Ok(result);
        }
    }
}
