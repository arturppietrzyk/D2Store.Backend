using D2Store.Api.Features.Users.Domain;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Users;

public record LoginUserCommand(string Email, string Password) : IRequest<Result<string>>;

public class LoginUserHandler : IRequestHandler<LoginUserCommand, Result<string>>
{
    private readonly AppDbContext _dbContext;
    private readonly IValidator<LoginUserCommand> _validator;

    public LoginUserHandler(AppDbContext dbContext, IValidator<LoginUserCommand> validator)
    {
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
        var loginUser = LoginUserAsync(userResult.Value, request, cancellationToken);
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

    private Result<string> LoginUserAsync(User user, LoginUserCommand request, CancellationToken cancellationToken)
    {
        if(new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
        {
            return Result.Failure<string>(new Error(
                "LoginUser.Validation",
                "The Password is wrong."));
        }
        string token = "sucess";
        return Result.Success(token);
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
