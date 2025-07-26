using D2Store.Api.Config;
using D2Store.Api.Features.Users;
using D2Store.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

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
            Secret = "secret",
            ExpiryMinutes = 60,
            Issuer = "issuer",
            Audience = "audience"
        };
        _jwtSettingsConfig = Options.Create(jwtSettingsConfig);
        var validator = new LoginUserCommandValidator();
        _sut = new LoginUserHandler(_jwtSettingsConfig, _dbContext, validator);
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenValidationFails()
    {
        // Arrange
        var command = new LoginUserCommand("", "");
        // Act
        var result = await _sut.Handle(command, default);
        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("LoginUser.Validation", result.Error.Code);
        Assert.Equal("Email is required.\r\nPassword is required.", result.Error.Message);
    }
}
