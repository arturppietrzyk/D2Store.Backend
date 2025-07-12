using D2Store.Api.Features.Orders.Dto;
using D2Store.Api.Shared;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

    [Authorize]
    [HttpPost("order")]
    public async Task<IActionResult> CreateOrder([FromBody] WriteOrderDtoCreate writeOrderDto)
    {
        var authenticatedUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("ADMIN");
        var result = await _mediator.Send(new CreateOrderCommand(writeOrderDto.UserId, writeOrderDto.Products, Guid.Parse(authenticatedUserId!), isAdmin));
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
    [HttpGet("order/{orderId}")]
    public async Task<IActionResult> GetOrder(Guid orderId)
    {
        var authenticatedUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("ADMIN");
        var result = await _mediator.Send(new GetOrderQuery(orderId, Guid.Parse(authenticatedUserId!), isAdmin));
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
    [HttpGet("orders/{userId}")]
    public async Task<IActionResult> GetOrdersForUser(Guid userId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var authenticatedUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("ADMIN");
        var result = await _mediator.Send(new GetOrdersForUserQuery(userId, pageNumber, pageSize, Guid.Parse(authenticatedUserId!), isAdmin));
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
    [HttpGet("orders")]
    public async Task<IActionResult> GetOrders([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var isAdmin = User.IsInRole("ADMIN");
        var result = await _mediator.Send(new GetOrdersQuery(pageNumber, pageSize, isAdmin));
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
    [HttpPatch("order/{orderId}")]
    public async Task<IActionResult> UpdateOrder(Guid orderId, [FromBody] WriteOrderDtoUpdate writeOrderDto)
    {
        var authenticatedUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("ADMIN");
        var result = await _mediator.Send(new UpdateOrderCommand(orderId, writeOrderDto.Status, Guid.Parse(authenticatedUserId!), isAdmin));
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
    [HttpDelete("order/{orderId}")]
    public async Task<IActionResult> DeleteOrder(Guid orderId)
    {
        var authenticatedUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("ADMIN");
        var result = await _mediator.Send(new DeleteOrderCommand(orderId, Guid.Parse(authenticatedUserId!), isAdmin));
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
