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
        //return CreatedAtAction(nameof(GetOrderById), new { id = orderId }, null);
    }

    //[HttpGet("order/{id}")]
    //public IActionResult GetOrderById(Guid id)
    //{
    //    return Ok(new { id, message = "Order retrieved successfully." });
    //}
}

