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
    public async Task<IActionResult> CreateProduct([FromBody] WriteProductDtoCreate writeProductDto)
    {
        var isAdmin = User.IsInRole(Role.ADMIN.ToString());
        var result = await _mediator.Send(new CreateProductCommand(writeProductDto.Name, writeProductDto.Description, writeProductDto.Price, writeProductDto.StockQuantity, isAdmin));
        if (result.IsFailure)
        {
            if (result.Error == Error.Forbidden)
            {
                return StatusCode(403, Error.Forbidden);
            }
            return BadRequest(result.Error);
        }
        return Ok(result.Value);
    }

    [HttpGet("product/{productId}")]
    public async Task<IActionResult> GetProductById(Guid productId)
    {
        var result = await _mediator.Send(new GetProductQuery(productId));
        if (result.IsFailure)
        {
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
    public async Task<IActionResult> UpdateProduct(Guid productId, [FromBody] WriteProductDtoUpdate writeProductDto)
    {
        var isAdmin = User.IsInRole(Role.ADMIN.ToString());
        var result = await _mediator.Send(new UpdateProductCommand(productId, writeProductDto.Name, writeProductDto.Description, writeProductDto.Price, writeProductDto.StockQuantity, isAdmin));
        if (result.IsFailure)
        {
            if (result.Error == Error.Forbidden)
            {
                return StatusCode(403, Error.Forbidden);
            }
            return BadRequest(result.Error);
        }
        return Ok(result.Value);
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
            return BadRequest(result.Error);
        }
        return Ok(result.Value);
    }
}
