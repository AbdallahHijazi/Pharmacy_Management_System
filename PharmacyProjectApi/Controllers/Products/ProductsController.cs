using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy.API.Infrastructure;
using Pharmacy.Application.DTOs.Products;
using Pharmacy.Application.Features.Products.Commands.CreateProduct;
using Pharmacy.Application.Features.Products.Commands.DeleteProduct;
using Pharmacy.Application.Features.Products.Commands.UpdateProduct;
using Pharmacy.Application.Features.Products.Queries.GetProductById;
using Pharmacy.Application.Features.Products.Queries.GetProducts;
using Pharmacy.Application.Features.Products.Queries.SearchProducts;

namespace PharmacyProjectApi.Controllers.Products
{
    [ApiController]
    [Authorize]
    [Route("api/v1/products")]
    public class ProductsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProductsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<ProductListItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetProducts()
        {
            var result = await _mediator.Send(new GetProductsQuery());
            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ProductDetailsDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequestDto request)
        {
            var result = await _mediator.Send(new CreateProductCommand
            {
                Name = request.Name,
                ScientificName = request.ScientificName,
                Barcode = request.Barcode,
                CategoryId = request.CategoryId,
                SellingPrice = request.SellingPrice,
                DefaultSupplierId = request.DefaultSupplierId
            });

            return CreatedAtAction(nameof(GetProductById), new { id = result.ProductId }, result);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ProductDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProductById(Guid id)
        {
            var result = await _mediator.Send(new GetProductByIdQuery
            {
                ProductId = id
            });

            return Ok(result);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ProductDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductRequestDto request)
        {
            var result = await _mediator.Send(new UpdateProductCommand
            {
                ProductId = id,
                Name = request.Name,
                ScientificName = request.ScientificName,
                Barcode = request.Barcode,
                CategoryId = request.CategoryId,
                SellingPrice = request.SellingPrice,
                DefaultSupplierId = request.DefaultSupplierId
            });

            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            await _mediator.Send(new DeleteProductCommand
            {
                ProductId = id
            });

            return Ok(new { message = "تم حذف المنتج بنجاح" });
        }

        [HttpGet("search")]
        [ProducesResponseType(typeof(List<ProductListItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SearchProducts([FromQuery] string query)
        {
            var result = await _mediator.Send(new SearchProductsQuery
            {
                Query = query
            });

            return Ok(result);
        }
    }
}
