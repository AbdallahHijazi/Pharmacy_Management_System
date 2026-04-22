using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy.API.Infrastructure;
using Pharmacy.Application.DTOs.Categories;
using Pharmacy.Application.Features.Categories.Commands.CreateCategory;
using Pharmacy.Application.Features.Categories.Commands.UpdateCategory;
using Pharmacy.Application.Features.Categories.Queries.GetCategories;
using Pharmacy.Application.Features.Categories.Queries.GetCategoryById;

namespace PharmacyProjectApi.Controllers.Categories
{

    [ApiController]
    [Authorize]
    [Route("api/v1/categories")]
    public class CategoriesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CategoriesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<CategoryListItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCategories()
        {
            var result = await _mediator.Send(new GetCategoriesQuery());
            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(CategoryListItemDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequestDto request)
        {
            var result = await _mediator.Send(new CreateCategoryCommand
            {
                Name = request.Name
            });

            return CreatedAtAction(nameof(GetCategoryById), new { id = result.CategoryId }, result);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(CategoryDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCategoryById(Guid id)
        {
            var result = await _mediator.Send(new GetCategoryByIdQuery
            {
                CategoryId = id
            });

            return Ok(result);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(CategoryDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryRequestDto request)
        {
            var result = await _mediator.Send(new UpdateCategoryCommand
            {
                CategoryId = id,
                Name = request.Name
            });

            return Ok(result);
        }
    }
}
