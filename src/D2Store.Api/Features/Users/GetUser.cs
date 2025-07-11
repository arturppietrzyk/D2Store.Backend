using D2Store.Api.Features.Users.Domain;
using D2Store.Api.Features.Users.Dto;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Users;

public record GetUserQuery(Guid UserId) : IRequest<Result<ReadUserDto>>;

public class GetUserHandler : IRequestHandler<GetUserQuery, Result<ReadUserDto>>
{
    private readonly AppDbContext _dbContext;

    public GetUserHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Coordinates retrieval and mapping of a specific user into a response DTO.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<Result<ReadUserDto>> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var userResult = await GetUserAsync(request.UserId, cancellationToken);
        if (userResult.IsFailure)
        {
            return Result.Failure<ReadUserDto>(userResult.Error);
        }
        return Result.Success(MapToReadUserDto(userResult.Value));
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
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<User>(new Error(
            "GetUser.Validation",
            "The user with the specified User Id was not found."));
        }
        return Result.Success(user);
    }

    /// <summary>
    /// Maps the retrieved user into the ReadUserDto which is returned as the response. 
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
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
