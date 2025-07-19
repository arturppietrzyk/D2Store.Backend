using D2Store.Api.Features.Users;
using D2Store.Api.Features.Users.Domain;
using D2Store.Api.Features.Users.Dto;
using D2Store.Api.Shared;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Security.Claims;

namespace D2Store.Tests.Unit.Controllers;

public class UserControllerTests
{
    private readonly UserController _sut;
    private readonly IMediator _mediator = Substitute.For<IMediator>();

    public UserControllerTests()
    {
        _sut = new UserController(_mediator);
    }

    // RegisterUser Tests
    [Fact]
    public async Task RegisterUser_ReturnsOkAndObject_WhenRegisterSuccessful()
    {
        // Arrange
        var writeUserDto = new WriteUserDtoRegister
        {
            FirstName = "Artur",
            LastName = "Pietrzyk",
            Email = "artur@example.com",
            Password = "Password",
            PhoneNumber = "123456",
            Address = "Address"
        };
        var user = User.Register(writeUserDto.FirstName, writeUserDto.LastName, writeUserDto.Email, writeUserDto.Email, writeUserDto.PhoneNumber, writeUserDto.Address);
        var readUserDto = new ReadUserDto(
            user.UserId,
            user.FirstName,
            user.LastName,
            user.Email,
            user.PasswordHash,
            user.PhoneNumber,
            user.Address,
            user.Role,
            user.CreatedDate,
            user.LastModified);
        var result = Result.Success(readUserDto);
        _mediator.Send(Arg.Is<RegisterUserCommand>(cmd =>
        cmd.FirstName == writeUserDto.FirstName &&
        cmd.LastName == writeUserDto.LastName &&
        cmd.Email == writeUserDto.Email &&
        cmd.Password == writeUserDto.Password &&
        cmd.PhoneNumber == writeUserDto.PhoneNumber &&
        cmd.Address == writeUserDto.Address),Arg.Any<CancellationToken>()).Returns(result);
        // Act
        var actionResult = await _sut.RegisterUser(writeUserDto);
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var returnedUser = Assert.IsType<ReadUserDto>(okResult.Value);
        Assert.Equal(result.Value.FirstName, returnedUser.FirstName);
    }

    [Fact]
    public async Task RegisterUser_ReturnsBadRequestAndError_WhenRegistrationFails()
    {
        var writeUserDto = new WriteUserDtoRegister
        {
            FirstName = "Artur",
            LastName = "Pietrzyk",
            Email = "artur@example.com",
            Password = "Password",
            PhoneNumber = "123456",
            Address = "Address"
        };
        var error = new Error("RegisterUser.Validation", "First Name is required.");
        var badRequestResult = Result.Failure<ReadUserDto>(error);
        _mediator.Send(Arg.Is<RegisterUserCommand>(cmd =>
        cmd.FirstName == writeUserDto.FirstName &&
        cmd.LastName == writeUserDto.LastName &&
        cmd.Email == writeUserDto.Email &&
        cmd.Password == writeUserDto.Password &&
        cmd.PhoneNumber == writeUserDto.PhoneNumber &&
        cmd.Address == writeUserDto.Address), Arg.Any<CancellationToken>()).Returns(badRequestResult);
        // Act
        var actionResult = await _sut.RegisterUser(writeUserDto);
        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(actionResult);
        Assert.Equal(400, badRequest.StatusCode);
        Assert.Equal(error, badRequest.Value);
    }

    //LoginUser
    //[Fact]
    //public async Task LoginUser_ReturnsOkAndObject_WhenLoginSuccessful()
    //{
    //    var writeUserDto = new WriteUserDtoLogin
    //    {
    //        Email = "artur@example.com",
    //        Password = "Password"
    //    };

    //}

    // GetUser Tests
    [Fact]
    public async Task GetUser_ReturnsOkAndObject_WhenUserExists()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var authenticatedUserId = userId;
        var user = User.Register("Artur", "Pietrzyk", "artur@example.com", "hashedPwd", "123456", "Address");
        var readUserDto = new ReadUserDto(
            user.UserId,
            user.FirstName,
            user.LastName,
            user.Email,
            user.PasswordHash,
            user.PhoneNumber,
            user.Address,
            user.Role,
            user.CreatedDate,
            user.LastModified);
        var result = Result.Success(readUserDto);
        _mediator.Send(Arg.Is<GetUserQuery>(q =>
        q.AuthenticatedUserId == userId &&
        q.AuthenticatedUserId == authenticatedUserId &&
        q.IsAdmin),Arg.Any<CancellationToken>()).Returns(result);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, authenticatedUserId.ToString()),
            new Claim(ClaimTypes.Role, "ADMIN")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
        // Act
        var actionResult = await _sut.GetUser(userId);
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var returnedUser = Assert.IsType<ReadUserDto>(okResult.Value);
        Assert.Equal(result.Value.FirstName, returnedUser.FirstName);
    }


    [Fact]
    public async Task GetUser_ReturnsForbiddenAndError_WhenUserIsForbidden()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var authenticatedUserId = Guid.CreateVersion7();
        var forbiddenResult = Result.Failure<ReadUserDto>(Error.Forbidden);
        _mediator.Send(Arg.Any<GetUserQuery>(), Arg.Any<CancellationToken>()).Returns(forbiddenResult);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, authenticatedUserId.ToString()),
            new Claim(ClaimTypes.Role, "CUSTOMER")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
        // Act
        var actionResult = await _sut.GetUser(userId);
        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(403, statusCodeResult.StatusCode);
        Assert.Equal(Error.Forbidden, statusCodeResult.Value);
    }

    [Fact]
    public async Task GetUser_ReturnsBadRequestAndError_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var authenticatedUserId = userId;
        var error = new Error("GetUser.Validation", "The user with the specified User Id was not found.");
        var badRequestResult = Result.Failure<ReadUserDto>(error);
        _mediator.Send(Arg.Any<GetUserQuery>(), Arg.Any<CancellationToken>()).Returns(badRequestResult);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, authenticatedUserId.ToString()),
            new Claim(ClaimTypes.Role, "CUSTOMER")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
        // Act
        var actionResult = await _sut.GetUser(userId);
        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(actionResult);
        Assert.Equal(400, badRequest.StatusCode);
        Assert.Equal(error, badRequest.Value);
    }
}
