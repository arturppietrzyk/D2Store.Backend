using D2Store.Api.Features.Products.Dto;
using MediatR;
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

    [HttpPost("product")]
    public async Task<IActionResult> CreateProduct([FromBody] WriteProductDtoCreate writeProductDto)
    {
        var result = await _mediator.Send(new CreateProductCommand(writeProductDto.Name, writeProductDto.Description, writeProductDto.Price, writeProductDto.StockQuantity));
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }
        return Ok(result.Value);
    }

    [HttpGet("product/{productId}")]
    public async Task<IActionResult> GetProductById(Guid productId)
    {
        var result = await _mediator.Send(new GetProductByIdQuery(productId));
        if (result.IsFailure)
        {
            return NotFound(result.Error);
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

    [HttpPatch("product/{productId}")]
    public async Task<IActionResult> UpdateProduct(Guid productId, [FromBody] WriteProductDtoUpdate writeProductDto)
    {
        var result = await _mediator.Send(new UpdateProductCommand(productId, writeProductDto.Name, writeProductDto.Description, writeProductDto.Price, writeProductDto.StockQuantity));
        if (result.IsFailure)
        {
            return NotFound(result.Error);
        }
        return Ok(result.Value);
    }

    [HttpDelete("product/{productId}")]
    public async Task<IActionResult> DeleteProduct(Guid productId)
    {
        var result = await _mediator.Send(new DeleteProductCommand(productId));
        if (result.IsFailure)
        {
            return NotFound(result.Error);
        }
        return Ok(result.Value);
    }
}
