using D2Store.Api.Config;
using D2Store.Api.Features.Users.Domain;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace D2Store.Api.Features.Users;

public record LoginUserCommand(string Email, string Password) : IRequest<Result<string>>;

public class LoginUserHandler : IRequestHandler<LoginUserCommand, Result<string>>
{
    private readonly JwtSettingsConfig _jwtSettingsConfig;
    private readonly AppDbContext _dbContext;
    private readonly IValidator<LoginUserCommand> _validator;

    public LoginUserHandler(IOptions<JwtSettingsConfig> jwtSettingsConfig, AppDbContext dbContext, IValidator<LoginUserCommand> validator)
    {
        _jwtSettingsConfig = jwtSettingsConfig.Value;
        _validator = validator;
        _dbContext = dbContext;
    }

    public async ValueTask<Result<string>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await ValidateRequestAsync(request, cancellationToken);
        if (validationResult.IsFailure)
        {
            return Result.Failure<string>(validationResult.Error);
        }
        var userResult = await GetUserAsync(request.Email, cancellationToken);
        if (userResult.IsFailure)
        {
            return Result.Failure<string>(userResult.Error);
        }
        var loginUser = LoginUser(userResult.Value, request);
        if (loginUser.IsFailure)
        {
            return Result.Failure<string>(loginUser.Error);
        }
        return Result.Success(loginUser.Value);
    }

    private async Task<Result> ValidateRequestAsync(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure<User>(new Error("LoginUser.Validation", validationResult.ToString()));
        }
        return Result.Success();
    }

    private async Task<Result<User>> GetUserAsync(string email, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        if (user is null)
        {
            return Result.Failure<User>(new Error(
           "LoginUser.Validation",
           "The user with the specified Email is not found."));
        }
        return Result.Success(user);
    }

    private Result<string> LoginUser(User user, LoginUserCommand request)
    {
        if(new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
        {
            return Result.Failure<string>(new Error(
                "LoginUser.Validation",
                "The Password is wrong."));
        }
        string jwt = CreateJwt(user);
        return Result.Success(jwt);
    }

    private string CreateJwt(User user)
    {
        var claims = new Claim[]
        {
            new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettingsConfig.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
        var jwtDescriptor = new JwtSecurityToken(
            issuer: _jwtSettingsConfig.Issuer,
            audience: _jwtSettingsConfig.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettingsConfig.ExpiryMinutes),
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(jwtDescriptor);
    }
}

public class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(u => u.Email).NotEmpty().WithMessage("Email is required.");
        RuleFor(u => u.Password).NotEmpty().WithMessage("Password is required.");
    }
}
