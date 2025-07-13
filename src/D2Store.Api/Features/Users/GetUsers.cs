using D2Store.Api.Features.Users.Domain;
using D2Store.Api.Features.Users.Dto;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using FluentValidation;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Users;

public record GetUsersQuery(int PageNumber, int PageSize, bool IsAdmin) : IRequest<Result<List<ReadUserDto>>>;

public class GetUsersHandler : IRequestHandler<GetUsersQuery, Result<List<ReadUserDto>>>
{
    private readonly AppDbContext _dbContext;
    private readonly IValidator<GetUsersQuery> _validator;

    public GetUsersHandler(AppDbContext dbContext, IValidator<GetUsersQuery> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    /// <summary>
    /// Coordinates validation, retrieval and mapping of the specific users into response DTO.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<Result<List<ReadUserDto>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        if (!request.IsAdmin)
        {
            return Result.Failure<List<ReadUserDto>>(Error.Forbidden);
        }
        var validationResult = await ValidateRequestAsync(request, cancellationToken);
        if (validationResult.IsFailure)
        {
            return Result.Failure<List<ReadUserDto>>(validationResult.Error);
        }
        var users = await GetPaginatedUsersAsync(request.PageNumber, request.PageSize, cancellationToken);
        var userDtos = users.Select(MapToReadUserDto).ToList();
        return Result.Success(userDtos);
    }

    /// <summary>
    /// Validates the PageNumber and PageSize Pagination parameters. 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Result> ValidateRequestAsync(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure<List<ReadUserDto>>(new Error("GetUsers.Validation", validationResult.ToString()));
        }
        return Result.Success();
    }

    /// <summary>
    /// Loads the user objects based on the Pagination parameters.
    /// </summary>
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<List<User>> GetPaginatedUsersAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .OrderByDescending(c => c.CreatedDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Maps a User entity into a ReadUserDto.
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

public class GetUsersQueryValidator : AbstractValidator<GetUsersQuery>
{
    public GetUsersQueryValidator()
    {
        RuleFor(p => p.PageNumber).GreaterThan(0).WithMessage("Page Number must be greater than 0.");
        RuleFor(p => p.PageSize).GreaterThan(0).WithMessage("Page Size must be greater than 0.");
    }
}
