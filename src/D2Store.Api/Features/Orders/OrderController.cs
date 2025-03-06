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
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand command)
    {
        if (command == null)
        {
            return BadRequest("Invalid order data.");
        }
        var orderId = await _mediator.Send(command);
        return Ok(orderId);
    }

    [HttpGet("order/{id}")]
    public async Task<IActionResult> GetOrderById(Guid id)
    {
        var order = await _mediator.Send(new GetOrderByIdQuery(id));
        if (order == null)
        {
            return NotFound($"Order with ID {id} not found.");
        }
        return Ok(order);
    }

    [HttpGet("orders")]
    public async Task<IActionResult> GetOrders()
    {
        var orders = await _mediator.Send(new GetOrderQuery());
        if (orders == null || orders.Count == 0)
        {
            return NotFound("No orders found.");
        }
        return Ok(orders);
    }

    [HttpPut("order/{id}")]
    public async Task<IActionResult> UpdateOrder(Guid id, [FromBody] WriteOrderDto writeOrderDto)
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