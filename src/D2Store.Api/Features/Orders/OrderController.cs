using D2Store.Api.Features.Orders.Dto;
using MediatR;
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

    [HttpGet("order/{id}")]
    public async Task<IActionResult> GetOrderById(Guid id)
    {
        var result = await _mediator.Send(new GetOrderByIdQuery(id));
        if (result.IsFailure)
        {
            return NotFound(result.Error.Message);
        }
        return Ok(result.Value);
    }

    [HttpGet("orders")]
    public async Task<IActionResult> GetOrders()
    {
        var result = await _mediator.Send(new GetOrderQuery());
        if (result.IsFailure)
        {
            return NotFound(result.Error.Message);
        }
        return Ok(result.Value);
    }

    [HttpPut("order/{id}")]
    public async Task<IActionResult> UpdateOrder(Guid id, [FromBody] WriteOrderDtoUpdate writeOrderDto)
    {
        var result = await _mediator.Send(new UpdateOrderCommand(id, writeOrderDto.TotalAmount, writeOrderDto.Status));
        if (result.IsFailure)
        {
            return BadRequest(result.Error.Message);
        }
        return Ok(result.Value);
    }

    [HttpDelete("order/{id}")]
    public async Task<IActionResult> DeleteOrder(Guid id)
    {
        var result = await _mediator.Send(new DeleteOrderCommand(id));
        if (result.IsFailure)
        {
            return NotFound(result.Error.Message);
        }
        return Ok(result.Value);
    }
}
