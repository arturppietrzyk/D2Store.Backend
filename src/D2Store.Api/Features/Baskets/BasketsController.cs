using System.Security.Claims;
using D2Store.Api.Features.Baskets.Dto;
using D2Store.Api.Shared;
using D2Store.Api.Shared.Enums;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace D2Store.Api.Features.Baskets;

[ApiController]
[Route("api")]
public class BasketsController : ControllerBase
{
    private readonly IMediator _mediator;

    public BasketsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize]
    [HttpPost("baskets")]
    public async Task<IActionResult> UpsertBasket([FromBody] WriteBasketDtoUpsert dtoUpsert, CancellationToken cancellationToken)
    {
        var authenticatedUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole(Role.ADMIN.ToString());
        var result = await _mediator.Send(new UpsertBasketCommand(dtoUpsert.UserId, dtoUpsert.Product, Guid.Parse(authenticatedUserId!), isAdmin), cancellationToken);
        if (result.IsFailure)
        {
            if(result.Error == Error.Forbidden)
            {
                return StatusCode(403, Error.Forbidden);
            }
            return BadRequest(result.Error);
        }
        return Ok(result.Value);
    }

    [Authorize]
    [HttpGet("baskets/{basketId}")]
    public async Task<IActionResult> GetBasket(Guid basketId, CancellationToken cancellationToken)
    {
        var authenticatedUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole(Role.ADMIN.ToString());
        var result = await _mediator.Send(new GetBasketQuery(basketId, Guid.Parse(authenticatedUserId!), isAdmin), cancellationToken);
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
    [HttpDelete("baskets/{basketProductId}")]
    public async Task<IActionResult> DeleteBasket(Guid basketProductId, CancellationToken cancellationToken)
    {
        var authenticatedUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole(Role.ADMIN.ToString());
        var result = await _mediator.Send(new DeleteBasketCommand(basketProductId, Guid.Parse(authenticatedUserId!), isAdmin), cancellationToken);
        if(result.IsFailure)
        {
            if(result.Error == Error.Forbidden)
            {
                return StatusCode(403, Error.Forbidden);
            }
            if(result.Error == Error.NotFound)
            {
                return StatusCode(404, Error.NotFound);
            }
            return BadRequest(result.Error);
        }
        return NoContent();
    }

// create a method for deleting the basket directly and have the basket products be cascade deleted
// clean up the code so it fits this repo
// look into fluent validation again and do it for this code
// add logic for creating an order out of the stuff inside the basket
// add front end functionality to this
}