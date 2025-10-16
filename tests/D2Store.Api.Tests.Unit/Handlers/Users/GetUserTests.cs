using D2Store.Api.Features.Users;
using D2Store.Api.Features.Users.Domain;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Tests.Unit.Handlers.Users;

public class GetUserTests
{
    private readonly GetUserHandler _sut;
    private readonly AppDbContext _dbContext;

    public GetUserTests()
    {
        DbContextOptions<AppDbContext> dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
       .UseInMemoryDatabase(databaseName: Guid.CreateVersion7().ToString())
       .Options;
        _dbContext = new AppDbContext(dbContextOptions);
        _sut = new GetUserHandler(_dbContext);
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenUserNotAuthorized()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var authenticatedUserId = Guid.CreateVersion7();
        var otherUserId = Guid.CreateVersion7();
        var isAdmin = false;
        var query = new GetUserQuery(otherUserId, authenticatedUserId, isAdmin);
        // Act
        var result = await _sut.Handle(query, cancellationToken);
        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Error.Forbidden.Code, result.Error.Code);
        Assert.Equal(Error.Forbidden.Message, result.Error.Message);
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenUserDoesNotExist()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var nonExistentUserId = Guid.CreateVersion7();
        var authenticatedUserId = Guid.CreateVersion7();
        var isAdmin = true;
        var query = new GetUserQuery(nonExistentUserId, authenticatedUserId, isAdmin);
        // Act
        var result = await _sut.Handle(query, cancellationToken);
        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Error.NotFound.Code, result.Error.Code);
        Assert.Equal(Error.NotFound.Message, result.Error.Message);
    }

    [Fact]
    public async Task Handle_ReturnsSuccess_WhenUserExists()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var isAdmin = false;
        var existingUser = User.Register(
            "John",
            "Doe",
            "john@example.com",
            "Password1",
            "1234567890",
            "123 Street");
        var authenticatedUserId = existingUser.UserId;
        _dbContext.Users.Add(existingUser);
        await _dbContext.SaveChangesAsync(cancellationToken);
        var query = new GetUserQuery(existingUser.UserId, authenticatedUserId, isAdmin);
        // Act
        var result = await _sut.Handle(query, cancellationToken);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(existingUser.Email, result.Value.Email);
        Assert.Equal(existingUser.FirstName, result.Value.FirstName);
        Assert.Equal(existingUser.LastName, result.Value.LastName);
    }

    [Fact]
    public async Task Handle_ReturnsSuccess_WhenUserExistsAndIsAdminIsTrue()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var isAdmin = true;
        var authenticatedUserId = Guid.CreateVersion7();
        var existingUser = User.Register(
            "John",
            "Doe",
            "john@example.com",
            "Password1",
            "1234567890",
            "123 Street");
        _dbContext.Users.Add(existingUser);
        await _dbContext.SaveChangesAsync(cancellationToken);
        var query = new GetUserQuery(existingUser.UserId, authenticatedUserId, isAdmin);
        // Act
        var result = await _sut.Handle(query, cancellationToken);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(existingUser.Email, result.Value.Email);
        Assert.Equal(existingUser.FirstName, result.Value.FirstName);
        Assert.Equal(existingUser.LastName, result.Value.LastName);
    }
}
