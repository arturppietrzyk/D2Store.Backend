using D2Store.Api.Features.Orders.Domain;
using D2Store.Api.Features.Users;
using D2Store.Api.Features.Users.Domain;
using D2Store.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Tests.Unit.Handlers.Users;

public class DeleteUserTests
{
    private readonly DeleteUserHandler _sut;
    private readonly AppDbContext _dbContext;

    public DeleteUserTests()
    {
       DbContextOptions<AppDbContext> dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
      .UseInMemoryDatabase(databaseName: Guid.CreateVersion7().ToString())
      .Options;
        _dbContext = new AppDbContext(dbContextOptions);
        _sut = new DeleteUserHandler(_dbContext);
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenUserDoesNotExist()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var nonExistentUserId = Guid.CreateVersion7();
        var authenticatedUserId = Guid.CreateVersion7();
        var isAdmin = true;
        var command = new DeleteUserCommand(nonExistentUserId, authenticatedUserId, isAdmin);
        // Act
        var result = await _sut.Handle(command, cancellationToken);
        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("DeleteUser.Validation", result.Error.Code);
        Assert.Equal("The user with the specified User Id was not found.", result.Error.Message);
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenUserHasOrders()
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
        var usersOrder = Order.Create(
            existingUser.UserId,
            10);
        _dbContext.Orders.Add(usersOrder);
        await _dbContext.SaveChangesAsync(cancellationToken);
        var command = new DeleteUserCommand(existingUser.UserId, authenticatedUserId, isAdmin);
        // Act
        var result = await _sut.Handle(command, cancellationToken);
        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("User.Validation", result.Error.Code);
        Assert.Equal("User cannot be deleted because they have orders.", result.Error.Message);
    }

    [Fact]
    public async Task Handle_ReturnsSuccessAndUserId_WhenUserIsDeleted()
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
        var command = new DeleteUserCommand(existingUser.UserId, authenticatedUserId, isAdmin);
        // Act
        var result = await _sut.Handle(command, cancellationToken);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(existingUser.UserId.ToString(), result.Value.ToString());
    }
}
