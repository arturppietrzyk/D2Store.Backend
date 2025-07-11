using D2Store.Api.Features.Users.Dto;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace D2Store.Api.Features.Users;

[ApiController]
[Route("api/")]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register-user")]
    public async Task<IActionResult> RegisterUser([FromBody] WriteUserDtoRegister writeUserDto)
    {
        var result = await _mediator.Send(new RegisterUserCommand(writeUserDto.FirstName, writeUserDto.LastName, writeUserDto.Email, writeUserDto.Password, writeUserDto.PhoneNumber, writeUserDto.Address));
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }
        return Ok(result.Value);
    }

    [HttpPost("login-user")]
    public async Task<IActionResult> LoginUser([FromBody] WriteUserDtoLogin writeUserDto)
    {
        var result = await _mediator.Send(new LoginUserCommand(writeUserDto.Email, writeUserDto.Password));
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }
        return Ok(result.Value);
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUser(Guid userId)
    {
        var result = await _mediator.Send(new GetUserQuery(userId));
        if (result.IsFailure)
        {
            return NotFound(result.Error);
        }
        return Ok(result.Value);
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 5)
    {
        var result = await _mediator.Send(new GetUsersQuery(pageNumber, pageSize));
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }
        return Ok(result.Value);
    }

    [HttpDelete("user/{userId}")]
    public async Task<IActionResult> DeleteUser(Guid userId)
    {
        var result = await _mediator.Send(new DeleteUserCommand(userId));
        if (result.IsFailure)
        {
            return NotFound(result.Error);
        }
        return Ok(result.Value);
    }

    [HttpPatch("user/{userId}")]
    public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] WriteUserDtoUpdate writeUserDto)
    {
        var result = await _mediator.Send(new UpdateUserCommand(userId, writeUserDto.FirstName, writeUserDto.LastName, writeUserDto.Email, writeUserDto.PhoneNumber, writeUserDto.Address));
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }
        return Ok(result.Value);
    }

    [Authorize]
    [HttpGet("test-auth")]
    public  IActionResult AuthenticatedOnlyEndpoint()
    {
        return Ok("You are authenticated");
    }

    [Authorize(Roles = "ADMIN")]
    [HttpGet("admin-only")]
    public IActionResult AdminOnlyEndpoint()
    {
        return Ok("You are authorized");
    }

}
