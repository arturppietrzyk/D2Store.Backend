using D2Store.Api.Features.Orders.Dto;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace D2Store.Api.Features.Orders;

[ApiController]
[Route("api/")]
public class OrderController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrderController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("order")]
    public async Task<IActionResult> CreateOrder([FromBody] WriteOrderDtoCreate writeOrderDto)
    {
        var result = await _mediator.Send(new CreateOrderCommand(writeOrderDto.CustomerId, writeOrderDto.TotalAmount));
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }
        return Ok(result.Value);
    }

    [HttpGet("order/{orderId}")]
    public async Task<IActionResult> GetOrderById(Guid orderId)
    {
        var result = await _mediator.Send(new GetOrderByIdQuery(orderId));
        if (result.IsFailure)
        {
            return NotFound(result.Error);
        }
        return Ok(result.Value);
    }

    [HttpGet("orders")]
    public async Task<IActionResult> GetOrders([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetOrdersQuery(pageNumber, pageSize));
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }
        return Ok(result.Value);
    }

    [HttpPatch("order/{orderId}")]
    public async Task<IActionResult> UpdateOrder(Guid orderId, [FromBody] WriteOrderDtoUpdate writeOrderDto)
    {
        var result = await _mediator.Send(new UpdateOrderCommand(orderId, writeOrderDto.TotalAmount));
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }
        return Ok(result.Value);
    }

    [HttpDelete("order/{orderId}")]
    public async Task<IActionResult> DeleteOrder(Guid orderId)
    {
        var result = await _mediator.Send(new DeleteOrderCommand(orderId));
        if (result.IsFailure)
        {
            return NotFound(result.Error);
        }
        return Ok(result.Value);
    }
}
