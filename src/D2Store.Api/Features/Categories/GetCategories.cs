using D2Store.Api.Features.Categories.Dto;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Categories;

public record GetCategoriesQuery : IRequest<Result<IReadOnlyCollection<ReadCategoryDto>>>;

public class GetCategoriesHandler : IRequestHandler<GetCategoriesQuery, Result<IReadOnlyCollection<ReadCategoryDto>>>
{
    private readonly AppDbContext _dbContext;

    public GetCategoriesHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Coordinates the retrieval and mapping of the specific categories into a collection of response DTOs.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<Result<IReadOnlyCollection<ReadCategoryDto>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await GetCategoriesAsync(cancellationToken);
        var categoriesDto = categories.Select(MapToReadCategoryDto).ToList();
        return Result.Success<IReadOnlyCollection<ReadCategoryDto>>(categoriesDto);
    }

    /// <summary>
    /// Loads the category objects.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<IReadOnlyCollection<Category>> GetCategoriesAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Categories
        .AsNoTracking()
        .OrderByDescending(c => c.AddedDate)
        .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Maps an Category entity into a ReadCategoryDto. 
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