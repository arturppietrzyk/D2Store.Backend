using D2Store.Api.Features.Products.Dto;
using D2Store.Api.Shared;
using D2Store.Api.Shared.Enums;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace D2Store.Api.Features.Products;

[ApiController]
[Route("api/")]
public class ProductController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize]
    [HttpPost("product")]
    public async Task<IActionResult> CreateProduct([FromBody] WriteProductDtoCreate dtoCreate)
    {
        var isAdmin = User.IsInRole(Role.ADMIN.ToString());
        var result = await _mediator.Send(new CreateProductCommand(dtoCreate.Name, dtoCreate.Description, dtoCreate.Price, dtoCreate.StockQuantity, dtoCreate.Images, isAdmin));
        if (result.IsFailure)
        {
            if (result.Error == Error.Forbidden)
            {
                return StatusCode(403, Error.Forbidden);
            }
            return BadRequest(result.Error);
        }
        return CreatedAtAction(nameof(GetProductById), new { productId = result.Value.ProductId }, result.Value);
    }

    [HttpGet("product/{productId}")]
    public async Task<IActionResult> GetProductById(Guid productId)
    {
        var result = await _mediator.Send(new GetProductQuery(productId));
        if (result.IsFailure)
        {
            if (result.Error == Error.NotFound)
            {
                return StatusCode(404, Error.NotFound);
            }
            return BadRequest(result.Error);
        }
        return Ok(result.Value);
    }

    [HttpGet("products")]
    public async Task<IActionResult> GetProducts([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetProductsQuery(pageNumber, pageSize));
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }
        return Ok(result.Value);
    }

    [Authorize]
    [HttpPatch("product/{productId}")]
    public async Task<IActionResult> UpdateProduct(Guid productId, [FromBody] WriteProductDtoUpdate dtoUpdate)
    {
        var isAdmin = User.IsInRole(Role.ADMIN.ToString());
        var result = await _mediator.Send(new UpdateProductCommand(productId, dtoUpdate.Name, dtoUpdate.Description, dtoUpdate.Price, dtoUpdate.StockQuantity, isAdmin));
        if (result.IsFailure)
        {
            if (result.Error == Error.Forbidden)
            {
                return StatusCode(403, Error.Forbidden);
            }
            if (result.Error == Error.NotFound)
            {
                return StatusCode(404, Error.NotFound);
            }
            return BadRequest(result.Error);
        }
        return NoContent();
    }

    [Authorize]
    [HttpDelete("product/{productId}")]
    public async Task<IActionResult> DeleteProduct(Guid productId)
    {
        var isAdmin = User.IsInRole(Role.ADMIN.ToString());
        var result = await _mediator.Send(new DeleteProductCommand(productId, isAdmin));
        if (result.IsFailure)
        {
            if (result.Error == Error.Forbidden)
            {
                return StatusCode(403, Error.Forbidden);
            }
            if (result.Error == Error.NotFound)
            {
                return StatusCode(404, Error.NotFound);
            }
            return BadRequest(result.Error);
        }
        return NoContent();
    }

    [Authorize]
    [HttpPost("product/{productId}/images")]
    public async Task<IActionResult> AddProductImages(Guid productId, [FromBody] WriteProductImagesDtoAdd dtoAddImages)
    {
        var isAdmin = User.IsInRole(Role.ADMIN.ToString());
        var result = await _mediator.Send(new AddProductImagesCommand(productId, dtoAddImages.Images, isAdmin));
        if (result.IsFailure)
        {
            if (result.Error == Error.Forbidden)
            {
                return StatusCode(403, Error.Forbidden);
            }
            if (result.Error == Error.NotFound)
            {
                return StatusCode(404, Error.NotFound);
            }
            return BadRequest(result.Error);
        }
        return NoContent();
    }


    [Authorize]
    [HttpDelete("product/{productId}/images")]
    public async Task<IActionResult> RemoveProductImages(Guid productId, [FromBody] WriteProductImagesDtoRemove dtoRemoveImages)
    {
        var isAdmin = User.IsInRole(Role.ADMIN.ToString());
        var result = await _mediator.Send(new RemoveProductImagesCommand(productId, dtoRemoveImages.ProductImageIds, isAdmin));
        if (result.IsFailure)
        {
            if (result.Error == Error.Forbidden)
            {
                return StatusCode(403, Error.Forbidden);
            }
            if (result.Error == Error.NotFound)
            {
                return StatusCode(404, Error.NotFound);
            }
            return BadRequest(result.Error);
        }
        return NoContent();
    }

    [Authorize]
    [HttpPatch("product/{productId}/images/{productImageId}/primary")]
    public async Task<IActionResult> ChangePrimaryImage(Guid productId, Guid productImageId)
    {
        var isAdmin = User.IsInRole(Role.ADMIN.ToString());
        var result = await _mediator.Send(new ChangePrimaryImageCommand(productId, productImageId, isAdmin));
        if (result.IsFailure)
        {
            if (result.Error == Error.Forbidden)
            {
                return StatusCode(403, Error.Forbidden);
            }
            if (result.Error == Error.NotFound)
            {
                return StatusCode(404, Error.NotFound);
            }
            return BadRequest(result.Error);
        }
        return NoContent();
    }
}
