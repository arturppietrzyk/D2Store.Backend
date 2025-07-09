using D2Store.Api.Features.Users.Dto;
using Mediator;
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
}
