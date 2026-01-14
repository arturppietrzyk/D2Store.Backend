using D2Store.Api.Features.Orders.Dto;
using D2Store.Api.Shared;
using D2Store.Api.Shared.Enums;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace D2Store.Api.Features.Orders;

[ApiController]
[Route("api/")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize]
    [HttpPost("orders")]
    public async Task<IActionResult> CreateOrder([FromBody] WriteOrderDtoCreate dtoCreate, CancellationToken cancellationToken)
    {
        var authenticatedUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole(Role.ADMIN.ToString());
        var result = await _mediator.Send(new CreateOrderCommand(dtoCreate.UserId, dtoCreate.Products, Guid.Parse(authenticatedUserId!), isAdmin), cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error == Error.Forbidden)
            {
                return StatusCode(403, Error.Forbidden);
            }
            return BadRequest(result.Error);
        }
        return CreatedAtAction(nameof(GetOrder), new { orderId = result.Value.OrderId }, result.Value);
    }

    [Authorize]
    [HttpGet("orders/{orderId}")]
    public async Task<IActionResult> GetOrder(Guid orderId, CancellationToken cancellationToken)
    {
        var authenticatedUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole(Role.ADMIN.ToString());
        var result = await _mediator.Send(new GetOrderQuery(orderId, Guid.Parse(authenticatedUserId!), isAdmin), cancellationToken);
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
        return Ok(result.Value);
    }

    [Authorize]
    [HttpGet("users/{userId}/orders")]
    public async Task<IActionResult> GetOrdersForUser(Guid userId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var authenticatedUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole(Role.ADMIN.ToString());
        var result = await _mediator.Send(new GetOrdersForUserQuery(userId, pageNumber, pageSize, Guid.Parse(authenticatedUserId!), isAdmin), cancellationToken);
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
    public async Task<IActionResult> GetOrders([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var isAdmin = User.IsInRole(Role.ADMIN.ToString());
        var result = await _mediator.Send(new GetOrdersQuery(pageNumber, pageSize, isAdmin), cancellationToken);
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
    [HttpPatch("orders/{orderId}")]
    public async Task<IActionResult> UpdateOrder(Guid orderId, [FromBody] WriteOrderDtoUpdate dtoUpdate, CancellationToken cancellationToken)
    {
        var authenticatedUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole(Role.ADMIN.ToString());
        var result = await _mediator.Send(new UpdateOrderCommand(orderId, dtoUpdate.Status, Guid.Parse(authenticatedUserId!), isAdmin), cancellationToken);
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
    [HttpDelete("orders/{orderId}")]
    public async Task<IActionResult> DeleteOrder(Guid orderId, CancellationToken cancellationToken)
    {
        var authenticatedUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole(Role.ADMIN.ToString());
        var result = await _mediator.Send(new DeleteOrderCommand(orderId, Guid.Parse(authenticatedUserId!), isAdmin), cancellationToken);
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
