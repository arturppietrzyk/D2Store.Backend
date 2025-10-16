using D2Store.Api.Features.Users;
using D2Store.Api.Features.Users.Dto;
using D2Store.Api.Shared;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Security.Claims;

namespace D2Store.Api.Tests.Unit.Controllers;

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
    public async Task RegisterUser_ReturnsCreatedAtActionAndObject_WhenRegisterUserSucceeds()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var dtoRegister = new WriteUserDtoRegister
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Password = "Password",
            PhoneNumber = "123456",
            Address = "Address"
        };
        var userDto = new ReadUserDto(
            userId,
            "John,",
            "Doe",
            "john@example.com",
            "hashedPwd",
            "123456",
            "Address",
            "ADMIN",
            DateTime.Now,
            DateTime.Now);
        var result = Result.Success(userDto);
        _mediator.Send(Arg.Is<RegisterUserCommand>(cmd =>
        cmd.FirstName == dtoRegister.FirstName &&
        cmd.LastName == dtoRegister.LastName &&
        cmd.Email == dtoRegister.Email &&
        cmd.Password == dtoRegister.Password &&
        cmd.PhoneNumber == dtoRegister.PhoneNumber &&
        cmd.Address == dtoRegister.Address), Arg.Any<CancellationToken>()).Returns(result);
        // Act
        var actionResult = await _sut.RegisterUser(dtoRegister);
        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult);
        Assert.Equal(nameof(_sut.GetUser), createdAtActionResult.ActionName);
        var returnedUser = Assert.IsType<ReadUserDto>(createdAtActionResult.Value);
        Assert.Equal(result.Value.FirstName, returnedUser.FirstName);
    }

    [Fact]
    public async Task RegisterUser_ReturnsBadRequestAndError_WhenRegisterUserFails()
    {
        // Arrange
        var dtoRegister = new WriteUserDtoRegister
        {
            FirstName = "",
            LastName = "Doe",
            Email = "john@example.com",
            Password = "Password",
            PhoneNumber = "123456",
            Address = "Address"
        };
        var error = new Error("RegisterUser.Validation", "First Name is required.");
        var result = Result.Failure<ReadUserDto>(error);
        _mediator.Send(Arg.Is<RegisterUserCommand>(cmd =>
        cmd.FirstName == dtoRegister.FirstName &&
        cmd.LastName == dtoRegister.LastName &&
        cmd.Email == dtoRegister.Email &&
        cmd.Password == dtoRegister.Password &&
        cmd.PhoneNumber == dtoRegister.PhoneNumber &&
        cmd.Address == dtoRegister.Address), Arg.Any<CancellationToken>()).Returns(result);
        // Act
        var actionResult = await _sut.RegisterUser(dtoRegister);
        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult);
        Assert.Equal(400, badRequestResult.StatusCode);
        Assert.Equal(error, badRequestResult.Value);
    }

    //LoginUser Tests
    [Fact]
    public async Task LoginUser_ReturnsOkAndObject_WhenLoginUserSucceeds()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var authenticatedUserId = userId;
        var dtoLogin = new WriteUserDtoLogin
        {
            Email = "john@example.com",
            Password = "Password"
        };
        var readAuthDto = new ReadAuthDto
        (
            "Token",
             DateTime.UtcNow.AddHours(1)
        );
        var result = Result.Success(readAuthDto);
        _mediator.Send(Arg.Is<LoginUserCommand>(cmd =>
        cmd.Email == dtoLogin.Email &&
        cmd.Password == dtoLogin.Password), Arg.Any<CancellationToken>()).Returns(result);
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
        var actionResult = await _sut.LoginUser(dtoLogin);
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var returnedToken = Assert.IsType<ReadAuthDto>(okResult.Value);
        Assert.Equal(result.Value.AccessToken, returnedToken.AccessToken);
    }

    [Fact]
    public async Task LoginUser_ReturnsNotFoundAndError_WhenUserNotFound()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var authenticatedUserId = userId;
        var dtoLogin = new WriteUserDtoLogin
        {
            Email = "john@example.com",
            Password = "Password"
        };
        var result = Result.Failure<ReadAuthDto>(Error.NotFound);
        _mediator.Send(Arg.Is<LoginUserCommand>(cmd =>
        cmd.Email == dtoLogin.Email &&
        cmd.Password == dtoLogin.Password), Arg.Any<CancellationToken>()).Returns(result);
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
        var actionResult = await _sut.LoginUser(dtoLogin);
        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(404, statusCodeResult.StatusCode);
        Assert.Equal(Error.NotFound, statusCodeResult.Value);
    }

    [Fact]
    public async Task LoginUser_ReturnsBadRequestAndError_WhenLoginUserFails()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var authenticatedUserId = userId;
        var dtoLogin = new WriteUserDtoLogin
        {
            Email = "",
            Password = "Password"
        };
        var error = new Error("LoginUser.Validation", "Email is required.");
        var result = Result.Failure<ReadAuthDto>(error);
        _mediator.Send(Arg.Is<LoginUserCommand>(cmd =>
        cmd.Email == dtoLogin.Email &&
        cmd.Password == dtoLogin.Password), Arg.Any<CancellationToken>()).Returns(result);
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
        var actionResult = await _sut.LoginUser(dtoLogin);
        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult);
        Assert.Equal(400, badRequestResult.StatusCode);
        Assert.Equal(error, badRequestResult.Value);
    }

    // GetUser Tests
    [Fact]
    public async Task GetUser_ReturnsOkAndObject_WhenGetUserSucceeds()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var authenticatedUserId = userId;
        var dtoUser = new ReadUserDto(
            userId,
            "John,",
            "Doe",
            "john@example.com",
            "hashedPwd",
            "123456",
            "Address",
            "ADMIN",
            DateTime.Now,
            DateTime.Now);
        var result = Result.Success(dtoUser);
        _mediator.Send(Arg.Is<GetUserQuery>(q =>
        q.AuthenticatedUserId == userId &&
        q.AuthenticatedUserId == authenticatedUserId &&
        q.IsAdmin), Arg.Any<CancellationToken>()).Returns(result);
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
    public async Task GetUser_ReturnsForbiddenAndError_WhenUserNotAuthorized()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var authenticatedUserId = Guid.CreateVersion7();
        var result = Result.Failure<ReadUserDto>(Error.Forbidden);
        _mediator.Send(Arg.Any<GetUserQuery>(), Arg.Any<CancellationToken>()).Returns(result);
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
    public async Task GetUser_ReturnsNotFoundAndError_WhenUserNotFound()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var authenticatedUserId = userId;
        var result = Result.Failure<ReadUserDto>(Error.NotFound);
        _mediator.Send(Arg.Any<GetUserQuery>(), Arg.Any<CancellationToken>()).Returns(result);
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
        Assert.Equal(404, statusCodeResult.StatusCode);
        Assert.Equal(Error.NotFound, statusCodeResult.Value);
    }

    [Fact]
    public async Task GetUser_ReturnsBadRequestAndError_WhenGetUserFails()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var authenticatedUserId = userId;
        var error = new Error("GetUser", "Some Generic Error.");
        var result = Result.Failure<ReadUserDto>(error);
        _mediator.Send(Arg.Any<GetUserQuery>(), Arg.Any<CancellationToken>()).Returns(result);
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
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult);
        Assert.Equal(400, badRequestResult.StatusCode);
        Assert.Equal(error, badRequestResult.Value);
    }

    //GetUsers Tests
    [Fact]
    public async Task GetUsers_ReturnsOkAndObject_WhenGetUsersSucceeds()
    {
        // Arrange
        int pageNumber = 1;
        int pageSize = 2;
        var readUserDto1 = new ReadUserDto(
            Guid.CreateVersion7(),
            "John",
            "Doe",
            "john@example.com",
            "PasswordHash",
            "123415",
            "Address",
            "ADMIN",
            DateTime.Now,
            DateTime.Now
            );
        var readUserDto2 = new ReadUserDto(
            Guid.CreateVersion7(),
            "Jane",
            "Smith",
            "jane@example.com",
            "PasswordHash",
            "123415",
            "Address",
            "ADMIN",
            DateTime.Now,
            DateTime.Now);
        var resultList = new List<ReadUserDto>() { readUserDto1, readUserDto2 };
        var result = Result.Success<IReadOnlyCollection<ReadUserDto>>(resultList);
        _mediator.Send(Arg.Is<GetUsersQuery>(q => q.PageNumber == pageNumber && q.PageSize == pageSize && q.IsAdmin == true), Arg.Any<CancellationToken>()).Returns(result);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, "ADMIN")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
        // Act
        var actionResult = await _sut.GetUsers(pageNumber, pageSize);
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var returnedUsers = Assert.IsType<List<ReadUserDto>>(okResult.Value);
        Assert.Equal(result.Value.Count, returnedUsers.Count);
    }

    [Fact]
    public async Task GetUsers_ReturnsForbiddenAndError_WhenUserNotAuthorized()
    {
        // Arrange
        int pageNumber = 1;
        int pageSize = 2;
        var result = Result.Failure<IReadOnlyCollection<ReadUserDto>>(Error.Forbidden);
        _mediator.Send(Arg.Is<GetUsersQuery>(q => q.PageNumber == pageNumber && q.PageSize == pageSize && q.IsAdmin == true), Arg.Any<CancellationToken>()).Returns(result);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, "ADMIN")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
        // Act
        var actionResult = await _sut.GetUsers(pageNumber, pageSize);
        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(403, statusCodeResult.StatusCode);
        Assert.Equal(Error.Forbidden, statusCodeResult.Value);
    }

    [Fact]
    public async Task GetUsers_ReturnsBadRequestAndError_GetUsersFails()
    {
        // Arrange
        int pageNumber = 0;
        int pageSize = 2;
        var error = new Error("GetUsers.Validation", "Page Number must be greater than 0.");
        var result = Result.Failure<IReadOnlyCollection<ReadUserDto>>(error);
        _mediator.Send(Arg.Is<GetUsersQuery>(q => q.PageNumber == pageNumber && q.PageSize == pageSize && q.IsAdmin == true), Arg.Any<CancellationToken>()).Returns(result);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, "ADMIN")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
        // Act
        var actionResult = await _sut.GetUsers(pageNumber, pageSize);
        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult);
        Assert.Equal(400, badRequestResult.StatusCode);
        Assert.Equal(error, badRequestResult.Value);
    }

    //UpdateUserTests
    [Fact]
    public async Task Update_ReturnsNoContent_WhenUpdateUserSucceeds()
    {
        // Arrange
        var writeUserDto = new WriteUserDtoUpdate
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            PhoneNumber = "123456",
            Address = "Address"
        };
        var userId = Guid.CreateVersion7();
        var authenticatedUserId = userId;
        var result = Result.Success();
        _mediator.Send(Arg.Is<UpdateUserCommand>(c => c.UserId == userId && c.FirstName == writeUserDto.FirstName && c.LastName == writeUserDto.LastName && c.Email == writeUserDto.Email && c.PhoneNumber == writeUserDto.PhoneNumber && c.Address == writeUserDto.Address && c.AuthenticatedUserId == authenticatedUserId && c.IsAdmin == true), Arg.Any<CancellationToken>()).Returns(result);
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
        var actionResult = await _sut.UpdateUser(userId, writeUserDto);
        // Assert
        Assert.IsType<NoContentResult>(actionResult);
    }

    [Fact]
    public async Task UpdateUser_ReturnsForbiddenAndError_WhenUserUnauthorized()
    {
        // Arrange
        var writeUserDto = new WriteUserDtoUpdate
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            PhoneNumber = "123456",
            Address = "Address"
        };
        var userId = Guid.CreateVersion7();
        var authenticatedUserId = userId;
        var result = Result.Failure<Guid>(Error.Forbidden);
        _mediator.Send(Arg.Is<UpdateUserCommand>(c => c.UserId == userId && c.FirstName == writeUserDto.FirstName && c.LastName == writeUserDto.LastName && c.Email == writeUserDto.Email && c.PhoneNumber == writeUserDto.PhoneNumber && c.Address == writeUserDto.Address && c.AuthenticatedUserId == authenticatedUserId && c.IsAdmin == true), Arg.Any<CancellationToken>()).Returns(result);
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
        var actionResult = await _sut.UpdateUser(userId, writeUserDto);
        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(403, statusCodeResult.StatusCode);
        Assert.Equal(Error.Forbidden, statusCodeResult.Value);
    }

     [Fact]
    public async Task UpdateUser_ReturnsNotFoundAndError_WhenUserNotFound()
    {
        // Arrange
        var writeUserDto = new WriteUserDtoUpdate
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            PhoneNumber = "123456",
            Address = "Address"
        };
        var userId = Guid.CreateVersion7();
        var authenticatedUserId = userId;
        var result = Result.Failure<Guid>(Error.NotFound);
        _mediator.Send(Arg.Is<UpdateUserCommand>(c => c.UserId == userId && c.FirstName == writeUserDto.FirstName && c.LastName == writeUserDto.LastName && c.Email == writeUserDto.Email && c.PhoneNumber == writeUserDto.PhoneNumber && c.Address == writeUserDto.Address && c.AuthenticatedUserId == authenticatedUserId && c.IsAdmin == true), Arg.Any<CancellationToken>()).Returns(result);
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
        var actionResult = await _sut.UpdateUser(userId, writeUserDto);
        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(404, statusCodeResult.StatusCode);
        Assert.Equal(Error.NotFound, statusCodeResult.Value);
    }

    [Fact]
    public async Task UpdateUser_ReturnsBadRequestAndError_UpdateUserFails()
    {
        // Arrange
        var writeUserDto = new WriteUserDtoUpdate
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            PhoneNumber = "123456",
            Address = "Address"
        };
        var userId = Guid.CreateVersion7();
        var authenticatedUserId = userId;
        var error = new Error("UpdateUser", "Some Generic Error.");
        var result = Result.Failure<Guid>(error);
        _mediator.Send(Arg.Is<UpdateUserCommand>(c => c.UserId == userId && c.FirstName == writeUserDto.FirstName && c.LastName == writeUserDto.LastName && c.Email == writeUserDto.Email && c.PhoneNumber == writeUserDto.PhoneNumber && c.Address == writeUserDto.Address && c.AuthenticatedUserId == authenticatedUserId && c.IsAdmin == true), Arg.Any<CancellationToken>()).Returns(result);
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
        var actionResult = await _sut.UpdateUser(userId, writeUserDto);
        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult);
        Assert.Equal(400, badRequestResult.StatusCode);
        Assert.Equal(error, badRequestResult.Value);
    }

    // //DeleteUser Tests
    [Fact]
    public async Task DeleteUser_ReturnsOkAndObject_WhenDeleteUserSucceeds()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var authenticatedUserId = userId;
        var result = Result.Success();
        _mediator.Send(Arg.Is<DeleteUserCommand>(c => c.UserId == userId && c.AuthenticatedUserId == authenticatedUserId && c.IsAdmin == true), Arg.Any<CancellationToken>()).Returns(result);
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
        var actionResult = await _sut.DeleteUser(userId);
        // Assert
        Assert.IsType<NoContentResult>(actionResult);
    }

    [Fact]
    public async Task DeleteUser_ReturnsForbiddenAndError_WhenUserNotAuthorized()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var authenticatedUserId = userId;
        var result = Result.Failure<Guid>(Error.Forbidden);
        _mediator.Send(Arg.Any<DeleteUserCommand>(), Arg.Any<CancellationToken>()).Returns(result);
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
        var actionResult = await _sut.DeleteUser(userId);
        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(403, statusCodeResult.StatusCode);
        Assert.Equal(Error.Forbidden, statusCodeResult.Value);
    }

       [Fact]
    public async Task DeleteUser_ReturnsNotFoundAndError_WhenUserNotFound()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var authenticatedUserId = userId;
        var result = Result.Failure<Guid>(Error.NotFound);
        _mediator.Send(Arg.Any<DeleteUserCommand>(), Arg.Any<CancellationToken>()).Returns(result);
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
        var actionResult = await _sut.DeleteUser(userId);
        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(404, statusCodeResult.StatusCode);
        Assert.Equal(Error.NotFound, statusCodeResult.Value);
    }

    [Fact]
    public async Task DeleteUser_ReturnsBadRequestAndError_DeleteUserFails()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var authenticatedUserId = userId;
        var error = new Error("DeleteUser", "Some Generic Error.");
        var result = Result.Failure<Guid>(error);
        _mediator.Send(Arg.Any<DeleteUserCommand>(), Arg.Any<CancellationToken>()).Returns(result);
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
        var actionResult = await _sut.DeleteUser(userId);
        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult);
        Assert.Equal(400, badRequestResult.StatusCode);
        Assert.Equal(error, badRequestResult.Value);
    }
}

