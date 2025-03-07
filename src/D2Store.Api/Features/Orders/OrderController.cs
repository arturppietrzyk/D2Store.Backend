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
        var orderId = await _mediator.Send(new CreateOrderCommand(writeOrderDto.CustomerId, writeOrderDto.TotalAmount));
        return Ok(orderId);
    }

    [HttpGet("order/{id}")]
    public async Task<IActionResult> GetOrderById(Guid id)
    {
        var result = await _mediator.Send(new GetOrderByIdQuery(id));
        if (result.IsFailure)
        {
            return NotFound(result.Error);
        }
        return Ok(result.Value);
    }

    [HttpGet("orders")]
    public async Task<IActionResult> GetOrders()
    {
        var orders = await _mediator.Send(new GetOrderQuery());
        return Ok(orders);
    }

    [HttpPut("order/{id}")]
    public async Task<IActionResult> UpdateOrder(Guid id, [FromBody] WriteOrderDtoUpdate writeOrderDto)
    {
        var updatedOrder = await _mediator.Send(new UpdateOrderCommand(id, writeOrderDto.TotalAmount, writeOrderDto.Status));
        return Ok(updatedOrder);
    }

    [HttpDelete("order/{id}")]
    public async Task<IActionResult> DeleteOrder(Guid id)
    {
        var orderId = await _mediator.Send(new DeleteOrderCommand(id));
        return Ok(orderId);
    }
}