using D2Store.Api.Features.Users.Dto;
using D2Store.Api.Shared;
using D2Store.Api.Shared.Enums;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace D2Store.Api.Features.Users;

[ApiController]
[Route("api/")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("users")]
    public async Task<IActionResult> RegisterUser([FromBody] WriteUserDtoRegister dtoRegister, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RegisterUserCommand(dtoRegister.FirstName, dtoRegister.LastName, dtoRegister.Email, dtoRegister.Password, dtoRegister.PhoneNumber, dtoRegister.Address), cancellationToken);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }
        return CreatedAtAction(nameof(GetUser), new { userId = result.Value.UserId }, result.Value);
    }

    [HttpPost("users/login")]
    public async Task<IActionResult> LoginUser([FromBody] WriteUserDtoLogin dtoLogin, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new LoginUserCommand(dtoLogin.Email, dtoLogin.Password), cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error == Error.NotFound)
            {
                return StatusCode(404, Error.NotFound);
            }
            return BadRequest(result.Error);
        }
        return Ok(result.Value);
    }

    [Authorize]
    [HttpGet("users/{userId}")]
    public async Task<IActionResult> GetUser(Guid userId, CancellationToken cancellationToken)
    {
        var authenticatedUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole(Role.ADMIN.ToString());
        var result = await _mediator.Send(new GetUserQuery(userId, Guid.Parse(authenticatedUserId!), isAdmin), cancellationToken);
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
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 5, CancellationToken cancellationToken = default)
    {
        var isAdmin = User.IsInRole(Role.ADMIN.ToString());
        var result = await _mediator.Send(new GetUsersQuery(pageNumber, pageSize, isAdmin), cancellationToken);
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
    [HttpPatch("users/{userId}")]
    public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] WriteUserDtoUpdate dtoUpdate, CancellationToken cancellationToken)
    {
        var authenticatedUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole(Role.ADMIN.ToString());
        var result = await _mediator.Send(new UpdateUserCommand(userId, dtoUpdate.FirstName, dtoUpdate.LastName, dtoUpdate.Email, dtoUpdate.PhoneNumber, dtoUpdate.Address, Guid.Parse(authenticatedUserId!), isAdmin), cancellationToken);
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
    [HttpDelete("users/{userId}")]
    public async Task<IActionResult> DeleteUser(Guid userId, CancellationToken cancellationToken)
    {
        var authenticatedUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole(Role.ADMIN.ToString());
        var result = await _mediator.Send(new DeleteUserCommand(userId, Guid.Parse(authenticatedUserId!), isAdmin), cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error == Error.Forbidden)
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
}
