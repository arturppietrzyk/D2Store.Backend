using D2Store.Api.Features.Users;
using D2Store.Api.Features.Users.Domain;
using D2Store.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Tests.Unit.Handlers.Users;

public class GetUsersTests
{
    private readonly GetUsersHandler _sut;
    private readonly AppDbContext _dbContext;

    public GetUsersTests()
    {
        DbContextOptions<AppDbContext> dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
       .UseInMemoryDatabase(databaseName: Guid.CreateVersion7().ToString())
       .Options;
        _dbContext = new AppDbContext(dbContextOptions);
        var validator = new GetUsersQueryValidator();
        _sut = new GetUsersHandler(_dbContext, validator);
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenValidationFailsDueToInvalidPageNumberAndPageSize()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var pageNumber = 0;
        var pageSize = 0;
        var isAdmin = true;
        var query = new GetUsersQuery(pageNumber, pageSize, isAdmin);
        // Act
        var result = await _sut.Handle(query, cancellationToken);
        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("GetUsers.Validation", result.Error.Code);
        Assert.Equal("Page Number must be greater than 0.\nPage Size must be greater than 0.", result.Error.Message);
    }

    [Fact]
    public async Task Handle_ReturnsSuccess_WhenValidationPasses()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var pageNumber = 1;
        var pageSize = 10;
        var isAdmin = true;
        var query = new GetUsersQuery(pageNumber, pageSize, isAdmin);
        // Act
        var result = await _sut.Handle(query, cancellationToken);
        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_ReturnsSuccessAndUsers_WhenValidationPassesAndUsersExists()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var pageNumber = 1;
        var pageSize = 10;
        var isAdmin = true;
        var existingUser1 = User.Register(
            "John",
            "Doe",
            "john@example.com",
            "Password1",
            "1234567890",
            "123 Street");
        var existingUser2 = User.Register(
           "Jane",
           "Smith",
           "jane@example.com",
           "Password2",
           "1234567890",
           "123 Street");
        _dbContext.Users.Add(existingUser1);
        _dbContext.Users.Add(existingUser2);
        await _dbContext.SaveChangesAsync(cancellationToken);
        var query = new GetUsersQuery(pageNumber, pageSize, isAdmin);
        // Act
        var result = await _sut.Handle(query, cancellationToken);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);
    }
}
