using D2Store.Api.Features.Users;
using D2Store.Api.Features.Users.Domain;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Tests.Unit.Handlers.Users;

public class UpdateUserTests
{
    private readonly UpdateUserHandler _sut;
    private readonly AppDbContext _dbContext;

    public UpdateUserTests()
    {
        DbContextOptions<AppDbContext> dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
       .UseInMemoryDatabase(databaseName: Guid.CreateVersion7().ToString())
       .Options;
        _dbContext = new AppDbContext(dbContextOptions);
        var validator = new UpdateUserCommandValidator();
        _sut = new UpdateUserHandler(_dbContext, validator);
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenUserSendingUpdateCommandNotAuthorized()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var existingUserId = Guid.CreateVersion7();
        var authenticatedUserId = Guid.CreateVersion7();
        var isAdmin = false;
        var command = new UpdateUserCommand(
             existingUserId,
             FirstName: "John",
             LastName: "Doe",
             Email: "john@example.com",
             PhoneNumber: "1234567890",
             Address: "123 Street",
             authenticatedUserId,
             isAdmin);
        // Act
        var result = await _sut.Handle(command, cancellationToken);
        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Error.Forbidden.Code, result.Error.Code);
        Assert.Equal(Error.Forbidden.Message, result.Error.Message);
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenValidationFailsDueToEmptyUpdateValues()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var authenticatedUserId = Guid.CreateVersion7();
        var isAdmin = true;
        var existingUser = User.Register(
            "John",
            "Doe",
            "john@example.com",
            "Password1",
            "1234567890",
            "123 Street");
        _dbContext.Users.Add(existingUser);
        await _dbContext.SaveChangesAsync(cancellationToken);
        var command = new UpdateUserCommand(
            existingUser.UserId,
            FirstName: "",
            LastName: "",
            Email: "john@example.com",
            PhoneNumber: "1234567890",
            Address: "123 Street",
            authenticatedUserId,
            isAdmin);
        // Act
        var result = await _sut.Handle(command, cancellationToken);
        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("UpdateUser.Validation", result.Error.Code);
        Assert.Equal("First Name cannot be empty if provided.\nLast Name cannot be empty if provided.", result.Error.Message);
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenValidationFailsDueNullUpdateValues()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var authenticatedUserId = Guid.CreateVersion7();
        var isAdmin = true;
        var existingUser = User.Register(
            "John",
            "Doe",
            "john@example.com",
            "Password1",
            "1234567890",
            "123 Street");
        _dbContext.Users.Add(existingUser);
        await _dbContext.SaveChangesAsync(cancellationToken);
        var command = new UpdateUserCommand(
            existingUser.UserId,
            FirstName: null,
            LastName: null,
            Email: null,
            PhoneNumber: null,
            Address: null,
            authenticatedUserId,
            isAdmin);
        // Act
        var result = await _sut.Handle(command, cancellationToken);
        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("UpdateUser.Validation", result.Error.Code);
        Assert.Equal("At least one field (First Name, Last Name, Email, Phone Number, Address) must be provided.", result.Error.Message);
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenUserDoesNotExist()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var nonExistentUserId = Guid.CreateVersion7();
        var authenticatedUserId = Guid.CreateVersion7();
        var isAdmin = true;
        var command = new UpdateUserCommand(
             nonExistentUserId,
             FirstName: "John",
             LastName: "Doe",
             Email: "john@example.com",
             PhoneNumber: "1234567890",
             Address: "123 Street",
             authenticatedUserId,
             isAdmin);
        // Act
        var result = await _sut.Handle(command, cancellationToken);
        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Error.NotFound.Code, result.Error.Code);
        Assert.Equal(Error.NotFound.Message, result.Error.Message);
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenEmailInUseIsTrue()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var authenticatedUserId = Guid.CreateVersion7();
        var isAdmin = true;
        var existingUser1 = User.Register(
            "John",
            "Doe",
            "john@example.com",
            "HashedPassword",
            "1234567890",
            "123 Street");
        var existingUser2 = User.Register(
            "John",
            "Smith",
            "john@example1.com",
            "HashedPassword",
            "1234567890",
            "123 Street");
        _dbContext.Users.Add(existingUser1);
        _dbContext.Users.Add(existingUser2);
        await _dbContext.SaveChangesAsync(cancellationToken);
        var command = new UpdateUserCommand(
              existingUser1.UserId,
              FirstName: "John",
              LastName: "Doe",
              Email: "john@example1.com",
              PhoneNumber: "1234567890",
              Address: "124 Street",
              authenticatedUserId,
              isAdmin);
        // Act
        var result = await _sut.Handle(command, cancellationToken);
        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("User.Validation", result.Error.Code);
        Assert.Equal("User email already in use.", result.Error.Message);
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenUpdateValuesDoNotChangeToCurrentValues()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var authenticatedUserId = Guid.CreateVersion7();
        var isAdmin = true;
        var existingUser = User.Register(
            "John",
            "Doe",
            "john@example.com",
            "HashedPassword",
            "1234567890",
            "123 Street");
        _dbContext.Users.Add(existingUser);
        await _dbContext.SaveChangesAsync(cancellationToken);
        var command = new UpdateUserCommand(
              existingUser.UserId,
              FirstName: "John",
              LastName: "Doe",
              Email: "john@example.com",
              PhoneNumber: "1234567890",
              Address: "123 Street",
              authenticatedUserId,
              isAdmin);
        // Act
        var result = await _sut.Handle(command, cancellationToken);
        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("User.Validation", result.Error.Code);
        Assert.Equal("The changes are no different to what is currently there.", result.Error.Message);
    }

    [Fact]
    public async Task Handle_ReturnsSuccessAndUserId_WhenUserIsUpdated()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var authenticatedUserId = Guid.CreateVersion7();
        var isAdmin = true;
        var existingUser = User.Register(
            "John",
            "Doe",
            "john@example.com",
            "Password1",
            "1234567890",
            "123 Street");
        _dbContext.Users.Add(existingUser);
        await _dbContext.SaveChangesAsync(cancellationToken);
        var command = new UpdateUserCommand(
              existingUser.UserId,
              FirstName: "John",
              LastName: "Doe",
              Email: "john@example.com",
              PhoneNumber: "1234567890",
              Address: "124 Street",
              authenticatedUserId,
              isAdmin);
        // Act
        var result = await _sut.Handle(command, cancellationToken);
        // Assert
        Assert.True(result.IsSuccess);
    }
 }
