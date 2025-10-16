using D2Store.Api.Features.Users.Domain;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Users;

public record DeleteUserCommand(Guid UserId, Guid AuthenticatedUserId, bool IsAdmin) : IRequest<Result>;

public class DeleteUserHandler : IRequestHandler<DeleteUserCommand, Result>
{
    private readonly AppDbContext _dbContext;

    public DeleteUserHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Coordinates retrieval, validation and deletion of a specific user.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        if (!request.IsAdmin && request.UserId != request.AuthenticatedUserId)
        {
            return Result.Failure(Error.Forbidden);
        }
        var userResult = await GetUserAsync(request.UserId, cancellationToken);
        if (userResult.IsFailure)
        {
            return Result.Failure(userResult.Error);
        }
        var hasOrders = await _dbContext.Orders.AsNoTracking().AnyAsync(o => o.UserId == request.UserId, cancellationToken);
        var assertUserHasNoOrdersResult = User.AssertUserHasNoOrders(hasOrders);
        if (assertUserHasNoOrdersResult.IsFailure)
        {
            return Result.Failure(assertUserHasNoOrdersResult.Error);
        }
        await DeleteUserAsync(userResult.Value, cancellationToken);
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
    /// Deletes the specified user, persisting the changes to the database table. 
    /// </summary>
    /// <param name="user"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task DeleteUserAsync(User user, CancellationToken cancellationToken)
    {
        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
