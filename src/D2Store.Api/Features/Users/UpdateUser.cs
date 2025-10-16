using D2Store.Api.Features.Users.Domain;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using FluentValidation;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Users;

public record UpdateUserCommand(Guid UserId, string? FirstName, string? LastName, string? Email, string? PhoneNumber, string? Address, Guid AuthenticatedUserId, bool IsAdmin) : IRequest<Result>;

public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, Result>
{
    private readonly AppDbContext _dbContext;
    private readonly IValidator<UpdateUserCommand> _validator;

    public UpdateUserHandler(AppDbContext dbContext, IValidator<UpdateUserCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    /// <summary>
    /// Coordinates validation, retrieval, and updating of a specific user.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<Result> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        if (!request.IsAdmin && request.UserId != request.AuthenticatedUserId)
        {
            return Result.Failure(Error.Forbidden);
        }
        var validationResult = await ValidateRequestAsync(request, cancellationToken);
        if (validationResult.IsFailure)
        {
            return Result.Failure(validationResult.Error);
        }
        var userResult = await GetUserAsync(request.UserId, cancellationToken);
        if (userResult.IsFailure)
        {
            return Result.Failure(userResult.Error);
        }
        if (request.Email is not null)
        {
            var emailInUse = await _dbContext.Users.AsNoTracking().AnyAsync(u => u.Email == request.Email && u.UserId != request.UserId, cancellationToken);
            var assertUserEmailIsUniqueResult = User.AssertUserEmailIsUnique(emailInUse);
            if (assertUserEmailIsUniqueResult.IsFailure)
            {
                return Result.Failure(assertUserEmailIsUniqueResult.Error);
            }
        }
        var updateUserResult = await UpdateUserAsync(userResult.Value, request, cancellationToken);
        if (updateUserResult.IsFailure)
        {
            return Result.Failure(updateUserResult.Error);
        }
        return Result.Success();
    }

    /// <summary>
    /// Validates the input.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Result> ValidateRequestAsync(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure(new Error("UpdateUser.Validation", validationResult.ToString()));
        }
        return Result.Success();
    }

    /// <summary>
    /// Loads a user object based on the UserId.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Result<User>> GetUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<User>(Error.NotFound);
        }
        return Result.Success(user);
    }

    /// <summary>
    /// Updates the user, persisting the changes in the database table. 
    /// </summary>
    /// <param name="user"></param>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Result> UpdateUserAsync(User user, UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var isUpdatedResult = user.Update(request.FirstName, request.LastName, request.Email, request.PhoneNumber, request.Address);
        if (isUpdatedResult.IsFailure)
        {
            return Result.Failure(isUpdatedResult.Error);    
        }
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(u => u.FirstName).NotEmpty().When(u => u.FirstName is not null).WithMessage("First Name cannot be empty if provided.");
        RuleFor(u => u.LastName).NotEmpty().When(u => u.LastName is not null).WithMessage("Last Name cannot be empty if provided.");
        RuleFor(u => u.Email).NotEmpty().When(u => u.Email is not null).WithMessage("Email cannot be empty if provided.");
        RuleFor(u => u.PhoneNumber).NotEmpty().When(u => u.PhoneNumber is not null).WithMessage("Phone Number cannot be empty if provided.");
        RuleFor(u => u.Address).NotEmpty().When(u => u.Address is not null).WithMessage("Address cannot be empty if provided.");
        RuleFor(u => u)
            .Must(u =>
            u.FirstName is not null ||
            u.LastName is not null ||
            u.Email is not null ||
            u.PhoneNumber is not null ||
            u.Address is not null)
            .WithMessage("At least one field (First Name, Last Name, Email, Phone Number, Address) must be provided.");
    }
}
