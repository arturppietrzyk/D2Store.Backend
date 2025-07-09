using D2Store.Api.Features.Users.Domain;
using D2Store.Api.Features.Users.Dto;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Users;

public record RegisterUserCommand(string FirstName, string LastName, string Email, string Password, string PhoneNumber, string Address) :  IRequest<Result<ReadUserDto>>;

public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, Result<ReadUserDto>>
{
    private readonly AppDbContext _dbContext;
    private readonly IValidator<RegisterUserCommand> _validator;

    public RegisterUserHandler(AppDbContext dbContext, IValidator<RegisterUserCommand> validator)
    { 
        _dbContext = dbContext;
        _validator = validator;
    }

    public async ValueTask<Result<ReadUserDto>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await ValidateRequestAsync(request, cancellationToken);
        if (validationResult.IsFailure)
        {
            return Result.Failure<ReadUserDto>(validationResult.Error);
        }
        var emailInUse = await _dbContext.Users.AsNoTracking().AnyAsync(u => u.Email == request.Email, cancellationToken);
        var validateEmailUniquenessResult = User.ValidateEmailUniqueness(emailInUse);
        if (validateEmailUniquenessResult.IsFailure)
        {
            return Result.Failure<ReadUserDto>(validateEmailUniquenessResult.Error);
        }
        var registerUser = await RegisterUserAsync(request, cancellationToken);
        var userDto = MapToReadUserDto(registerUser);
        return Result.Success(userDto);
    }

    private async Task<Result> ValidateRequestAsync(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure<User>(new Error("RegisterUser.Validation", validationResult.ToString()));
        }
        return Result.Success();
    }

    private async Task<User> RegisterUserAsync(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var hashedPassword = new PasswordHasher<User>().HashPassword(null!, request.Password);
        var registerUser = User.Register(request.FirstName, request.LastName, request.Email, hashedPassword, request.PhoneNumber, request.Address);
        _dbContext.Users.Add(registerUser);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return registerUser;
    }

    private static ReadUserDto MapToReadUserDto(User user)
    {
        return new ReadUserDto(
            user.UserId,
            user.FirstName,
            user.LastName,
            user.Email,
            user.PasswordHash,
            user.PhoneNumber,
            user.Address,
            user.Role,
            user.CreatedDate,
            user.LastModified);
    }
}

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(u => u.FirstName).NotEmpty().WithMessage("First Name is required.");
        RuleFor(u => u.LastName).NotEmpty().WithMessage("Last Name is required.");
        RuleFor(u => u.Email).NotEmpty().WithMessage("Email is required.");
        RuleFor(u => u.Password).NotEmpty().WithMessage("Password is required.");
        RuleFor(u => u.PhoneNumber).NotEmpty().WithMessage("Phone Number is required.");
        RuleFor(u => u.Address).NotEmpty().WithMessage("Address is required.");
    }
}
