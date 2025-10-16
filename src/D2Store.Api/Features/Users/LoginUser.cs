using D2Store.Api.Config;
using D2Store.Api.Features.Users.Domain;
using D2Store.Api.Features.Users.Dto;
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

public record LoginUserCommand(string Email, string Password) : IRequest<Result<ReadAuthDto>>;

public class LoginUserHandler : IRequestHandler<LoginUserCommand, Result<ReadAuthDto>>
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

    /// <summary>
    /// Coordinates validation, mapping and login of a user. Returns the auth object in a form of a response DTO.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<Result<ReadAuthDto>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await ValidateRequestAsync(request, cancellationToken);
        if (validationResult.IsFailure)
        {
            return Result.Failure<ReadAuthDto>(validationResult.Error);
        }
        var userResult = await GetUserAsync(request.Email, cancellationToken);
        if (userResult.IsFailure)
        {
            return Result.Failure<ReadAuthDto>(userResult.Error);
        }
        var loginUserResult = LoginUser(userResult.Value, request);
        if (loginUserResult.IsFailure)
        {
            return Result.Failure<ReadAuthDto>(loginUserResult.Error);
        }
        var authDto = MapToReadAuthDto(loginUserResult.Value);
        return Result.Success(authDto);
    }

    /// <summary>
    /// Validates the input. 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Result> ValidateRequestAsync(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure(new Error("LoginUser.Validation", validationResult.ToString()));
        }
        return Result.Success();
    }

    /// <summary>
    /// Loads a user object based on the email.
    /// </summary>
    /// <param name="email"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Result<User>> GetUserAsync(string email, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        if (user is null)
        {
            return Result.Failure<User>(Error.NotFound);
        }
        return Result.Success(user);
    }

    /// <summary>
    /// Orchastres the login process.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    private Result<Auth> LoginUser(User user, LoginUserCommand request)
    {
        if (new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
        {
            return Result.Failure<Auth>(new Error("LoginUser.Validation", "The Password is wrong."));
        }
        string jwt = CreateJwt(user);
        var auth = Auth.Login(jwt, DateTime.UtcNow.AddMinutes(_jwtSettingsConfig.ExpiryMinutes));
        return Result.Success(auth);
    }

    /// <summary>
    /// Creates the JWT token. 
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    private string CreateJwt(User user)
    {
        var claims = new Claim[]
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Email, user.Email),
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

    /// <summary>
    /// Maps the auth object to the equvilant ReadAuthDto. 
    /// </summary>
    /// <param name="auth"></param>
    /// <returns></returns>
    private static ReadAuthDto MapToReadAuthDto(Auth auth)
    {
        return new ReadAuthDto(
            auth.AccessToken,
            auth.ExpiresAt);
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
