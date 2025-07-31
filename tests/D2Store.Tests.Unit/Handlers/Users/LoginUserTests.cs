using D2Store.Api.Config;
using D2Store.Api.Features.Users;
using D2Store.Api.Features.Users.Domain;
using D2Store.Api.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace D2Store.Api.Tests.Unit.Handlers.Users;

public class LoginUserTests
{
    private readonly LoginUserHandler _sut;
    private readonly IOptions<JwtSettingsConfig> _jwtSettingsConfig;
    private readonly AppDbContext _dbContext;

    public LoginUserTests()
    {
        DbContextOptions<AppDbContext> dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
       .UseInMemoryDatabase(databaseName: Guid.CreateVersion7().ToString())
       .Options;
        _dbContext = new AppDbContext(dbContextOptions);
        JwtSettingsConfig jwtSettingsConfig = new JwtSettingsConfig()
        {
            Secret = "secret-secret-secret-secret-secret-secret-secret-secret-secret-secret-secret-secret-",
            ExpiryMinutes = 60,
            Issuer = "issuer",
            Audience = "audience"
        };
        _jwtSettingsConfig = Options.Create(jwtSettingsConfig);
        var validator = new LoginUserCommandValidator();
        _sut = new LoginUserHandler(_jwtSettingsConfig, _dbContext, validator);
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenValidationFailsDueToEmptyLoginValues()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var command = new LoginUserCommand("", "");
        // Act
        var result = await _sut.Handle(command, cancellationToken);
        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("LoginUser.Validation", result.Error.Code);
        Assert.Equal("Email is required.\r\nPassword is required.", result.Error.Message);
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenEmailIsIncorrect()
    {
        // Arrange 
        var cancellationToken = CancellationToken.None;
        var password = "Password1";
        var hashedPassword = new PasswordHasher<User>().HashPassword(null!, password);
        var existingUser = User.Register(
            "John",
            "Doe",
            "john@example.com",
            hashedPassword,
            "1234567890",
            "123 Street");
        _dbContext.Users.Add(existingUser);
        await _dbContext.SaveChangesAsync(cancellationToken);
        var command = new LoginUserCommand("john1@example.com", "Password1");
        // Act
        var result = await _sut.Handle(command, cancellationToken);
        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("LoginUser.Validation", result.Error.Code);
        Assert.Equal("The user with the specified Email is not found.", result.Error.Message);
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenPasswordIsIncorrect()
    {
        // Arrange 
        var cancellationToken = CancellationToken.None;
        var password = "Password1";
        var hashedPassword = new PasswordHasher<User>().HashPassword(null!, password);
        var existingUser = User.Register(
            "John",
            "Doe",
            "john@example.com",
            hashedPassword,
            "1234567890",
            "123 Street");
        _dbContext.Users.Add(existingUser);
        await _dbContext.SaveChangesAsync(cancellationToken);
        var command = new LoginUserCommand("john@example.com", "Password2");
        // Act
        var result = await _sut.Handle(command, cancellationToken);
        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("LoginUser.Validation", result.Error.Code);
        Assert.Equal("The Password is wrong.", result.Error.Message);
    }

    [Fact]
    public async Task Handle_ReturnsSuccessAndJWT_WhenEmailAndPasswordIsCorrect()
    {
        // Arrange 
        var cancellationToken = CancellationToken.None;
        var password = "Password1";
        var hashedPassword = new PasswordHasher<User>().HashPassword(null!, password);
        var existingUser = User.Register(
            "John",
            "Doe",
            "john@example.com",
            hashedPassword,
            "1234567890",
            "123 Street");
        _dbContext.Users.Add(existingUser);
        await _dbContext.SaveChangesAsync(cancellationToken);
        var command = new LoginUserCommand("john@example.com", "Password1");
        // Act
        var result = await _sut.Handle(command, cancellationToken);
        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_ReturnsJwtToken_WithExpectedClaims()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var password = "Password1";
        var hashedPassword = new PasswordHasher<User>().HashPassword(null!, password);
        var existingUser = User.Register(
            "John",
            "Doe",
            "john@example.com",
            hashedPassword,
            "1234567890",
            "123 Street");
        _dbContext.Users.Add(existingUser);
        await _dbContext.SaveChangesAsync(cancellationToken);
        var command = new LoginUserCommand("john@example.com", password);
        // Act
        var result = await _sut.Handle(command, cancellationToken);
        // Assert
        Assert.True(result.IsSuccess);
        var token = result.Value;
        Assert.False(string.IsNullOrWhiteSpace(token));
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        Assert.Equal(existingUser.Email, jwt.Claims.First(c => c.Type == ClaimTypes.Email).Value);
        Assert.Equal(existingUser.UserId.ToString(), jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
        Assert.Equal(existingUser.Role, jwt.Claims.First(c => c.Type == ClaimTypes.Role).Value);
    }
}
