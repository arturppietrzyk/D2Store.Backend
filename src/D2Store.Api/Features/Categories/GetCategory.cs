using D2Store.Api.Features.Categories.Dto;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Categories;

public record GetCategoryQuery(Guid CategoryId) : IRequest<Result<ReadCategoryDto>>;

public class GetCategoryHandler : IRequestHandler<GetCategoryQuery, Result<ReadCategoryDto>>
{
    private readonly AppDbContext _dbContext;

    public GetCategoryHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Coordinates retrieval and mapping of a specific category into a response DTO.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<Result<ReadCategoryDto>> Handle(GetCategoryQuery request, CancellationToken cancellationToken)
    {
        var categoryResult = await GetCategoryAsync(request.CategoryId, cancellationToken);
        if(categoryResult.IsFailure)
        {
            return Result.Failure<ReadCategoryDto>(categoryResult.Error);
        }
        return Result.Success(MapToReadCategoryDto(categoryResult.Value));
    }

    /// <summary>
    /// Loads a category object based on the CategoryId.
    /// </summary>
    /// <param name="categoryId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Result<Category>> GetCategoryAsync(Guid categoryId, CancellationToken cancellationToken)
    {
        var category = await _dbContext.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CategoryId == categoryId, cancellationToken);
        if (category is null)
        {
            return Result.Failure<Category>(Error.NotFound);
        }
        return Result.Success(category);
    }

    /// <summary>
    /// Maps the retrieved category into the ReadCategoryDto which is returned as the response.
    /// </summary>
    /// <param name="category"></param>
    /// <returns></returns>
    private static ReadCategoryDto MapToReadCategoryDto(Category category)
    {
        return new ReadCategoryDto(
            category.CategoryId,
            category.Name,
            category.AddedDate,
            category.LastModified
        );
    }
}