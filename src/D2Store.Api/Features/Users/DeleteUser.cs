using D2Store.Api.Features.Users.Domain;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Users;

public record DeleteUserCommand(Guid UserId) : IRequest<Result<Guid>>;

public class DeleteUserHandler : IRequestHandler<DeleteUserCommand, Result<Guid>>
{
    private readonly AppDbContext _dbContext;

    public DeleteUserHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Coordinates retrieval, validation and deletion of a specific user. Returns the Guid of the deleted user if successful. 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<Result<Guid>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var userResult = await GetUserAsync(request.UserId, cancellationToken);
        if (userResult.IsFailure)
        {
            return Result.Failure<Guid>(userResult.Error);
        }
        var hasOrders = await _dbContext.Orders.AsNoTracking().AnyAsync(o => o.UserId == request.UserId, cancellationToken);
        var validateOrdersExsistanceResult = User.ValidateOrdersExistance(hasOrders);
        if (validateOrdersExsistanceResult.IsFailure)
        {
            return Result.Failure<Guid>(validateOrdersExsistanceResult.Error);
        }
        var deleteUser = await DeleteUserAsync(userResult.Value, cancellationToken);
        return Result.Success(deleteUser);
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
            return Result.Failure<User>(new Error(
           "DeleteUser.Validation",
           "The user with the specified User Id was not found."));
        }
        return Result.Success(user);
    }

    /// <summary>
    /// Deletes the specified user, persisting the changes to the database table. 
    /// </summary>
    /// <param name="user"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Guid> DeleteUserAsync(User user, CancellationToken cancellationToken)
    {
        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return user.UserId;
    }
}
