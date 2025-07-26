using D2Store.Api.Features.Users;
using D2Store.Api.Features.Users.Domain;
using D2Store.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Tests.Unit.Handlers.Users;

public class RegisterUserTests
{
    private readonly RegisterUserHandler _sut;
    private readonly AppDbContext _dbContext;

    public RegisterUserTests()
    {
        DbContextOptions<AppDbContext> dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
       .UseInMemoryDatabase(databaseName: Guid.CreateVersion7().ToString())
       .Options;
        _dbContext = new AppDbContext(dbContextOptions);
        var validator = new RegisterUserCommandValidator();
        _sut = new RegisterUserHandler(_dbContext, validator);
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenValidationFails()
    {
        // Arrange
        var command = new RegisterUserCommand("", "", "", "", "", "");
        // Act
        var result = await _sut.Handle(command, default);
        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("RegisterUser.Validation", result.Error.Code);
        Assert.Equal("First Name is required.\r\nLast Name is required.\r\nEmail is required.\r\nPassword is required.\r\nPhone Number is required.\r\nAddress is required.", result.Error.Message);
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenEmailInUseIsTrue()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var existingUser = User.Register(
            "John",
            "Doe",
            "john@example.com",
            "SomeHashedPassword",
            "1234567890",
            "123 Street");
        _dbContext.Users.Add(existingUser);
        await _dbContext.SaveChangesAsync(cancellationToken);
        var command = new RegisterUserCommand(
            FirstName: "John",
            LastName: "Doe",
            Email: "john@example.com",
            Password: "StrongPassword123",
            PhoneNumber: "1234567890",
            Address: "123 Street");
        // Act
        var result = await _sut.Handle(command, cancellationToken);
        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("User.Validation", result.Error.Code);
        Assert.Equal("User email already in use.", result.Error.Message);
    }

    [Fact]
    public async Task Handle_ReturnsSuccess_WhenDataIsValid()
    {
        // Arrange
        var command = new RegisterUserCommand(
            FirstName: "John",
            LastName: "Doe",
            Email: "john@example.com",
            Password: "StrongPassword123",
            PhoneNumber: "1234567890",
            Address: "123 Street");
        // Act
        var result = await _sut.Handle(command, default);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("John", result.Value.FirstName);
    }
}
