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
    public async Task RegisterUser_ReturnsOkAndObject_WhenRegisterUserSucceeds()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var writeUserDto = new WriteUserDtoRegister
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Password = "Password",
            PhoneNumber = "123456",
            Address = "Address"
        };
        var readUserDto = new ReadUserDto(
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
        var result = Result.Success(readUserDto);
        _mediator.Send(Arg.Is<RegisterUserCommand>(cmd =>
        cmd.FirstName == writeUserDto.FirstName &&
        cmd.LastName == writeUserDto.LastName &&
        cmd.Email == writeUserDto.Email &&
        cmd.Password == writeUserDto.Password &&
        cmd.PhoneNumber == writeUserDto.PhoneNumber &&
        cmd.Address == writeUserDto.Address), Arg.Any<CancellationToken>()).Returns(result);
        // Act
        var actionResult = await _sut.RegisterUser(writeUserDto);
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var returnedUser = Assert.IsType<ReadUserDto>(okResult.Value);
        Assert.Equal(result.Value.FirstName, returnedUser.FirstName);
    }

    [Fact]
    public async Task RegisterUser_ReturnsBadRequestAndError_WhenRegisterUserFails()
    {
        var writeUserDto = new WriteUserDtoRegister
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
        cmd.FirstName == writeUserDto.FirstName &&
        cmd.LastName == writeUserDto.LastName &&
        cmd.Email == writeUserDto.Email &&
        cmd.Password == writeUserDto.Password &&
        cmd.PhoneNumber == writeUserDto.PhoneNumber &&
        cmd.Address == writeUserDto.Address), Arg.Any<CancellationToken>()).Returns(result);
        // Act
        var actionResult = await _sut.RegisterUser(writeUserDto);
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
        var writeUserDto = new WriteUserDtoLogin
        {
            Email = "john@example.com",
            Password = "Password"
        };
        var jwtToken = "Token";
        var result = Result.Success(jwtToken);
        _mediator.Send(Arg.Is<LoginUserCommand>(cmd =>
        cmd.Email == writeUserDto.Email &&
        cmd.Password == writeUserDto.Password), Arg.Any<CancellationToken>()).Returns(result);
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
        var actionResult = await _sut.LoginUser(writeUserDto);
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var returnedToken = Assert.IsType<string>(okResult.Value);
        Assert.Equal(jwtToken, returnedToken);
    }

    [Fact]
    public async Task LoginUser_ReturnsBadRequestAndError_WhenLoginUserFails()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var authenticatedUserId = userId;
        var writeUserDto = new WriteUserDtoLogin
        {
            Email = "",
            Password = "Password"
        };
        var error = new Error("LoginUser.Validation", "Email is required.");
        var result = Result.Failure<string>(error);
        _mediator.Send(Arg.Is<LoginUserCommand>(cmd =>
        cmd.Email == writeUserDto.Email &&
        cmd.Password == writeUserDto.Password), Arg.Any<CancellationToken>()).Returns(result);
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
        var actionResult = await _sut.LoginUser(writeUserDto);
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
        var readUserDto = new ReadUserDto(
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
        var result = Result.Success(readUserDto);
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
    public async Task GetUser_ReturnsForbiddenAndError_WhenGetUserIsForbidden()
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
    public async Task GetUser_ReturnsBadRequestAndError_GetUserFails()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var authenticatedUserId = userId;
        var error = new Error("GetUser.Validation", "The user with the specified User Id was not found.");
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
        var result = Result.Success(resultList);
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
    public async Task GetUsers_ReturnsForbiddenAndError_WhenGetUsersIsForbidden()
    {
        int pageNumber = 1;
        int pageSize = 2;
        var result = Result.Failure<List<ReadUserDto>>(Error.Forbidden);
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
        int pageNumber = 0;
        int pageSize = 2;
        var error = new Error("GetUsers.Validation", "Page Number must be greater than 0.");
        var result = Result.Failure<List<ReadUserDto>>(error);
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

    //DeleteUser Tests
    [Fact]
    public async Task DeleteUser_ReturnsOkAndObject_WhenDeleteUserSucceeds()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var authenticatedUserId = userId;
        var result = Result.Success(authenticatedUserId);
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
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var returnedGuid = Assert.IsType<Guid>(okResult.Value);
        Assert.Equal(result.Value, returnedGuid);
    }

    [Fact]
    public async Task DeleteUser_ReturnsForbiddenAndError_WhenDeleteUserIsForbidden()
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
    public async Task DeleteUser_ReturnsBadRequestAndError_DeleteUserFails()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var authenticatedUserId = userId;
        var error = new Error("DeleteUser.Validation", "The user with the specified User Id was not found.");
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

    //UpdateUserTests
    [Fact]
    public async Task Update_ReturnsOkAndObject_WhenUpdateUserSucceeds()
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
        var result = Result.Success(authenticatedUserId);
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
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var returnedGuid = Assert.IsType<Guid>(okResult.Value);
        Assert.Equal(result.Value, returnedGuid);
    }

    [Fact]
    public async Task UpdateUser_ReturnsForbiddenAndError_WhenUpdateUserIsForbidden()
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
        var error = new Error("UpdateUser.Validation", "The user with the specified User Id was not found.");
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
}

