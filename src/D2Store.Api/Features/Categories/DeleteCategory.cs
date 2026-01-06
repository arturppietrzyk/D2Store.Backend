using D2Store.Api.Features.Categories.Dto;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Categories;

public record DeleteCategoryCommand(Guid CategoryId, bool IsAdmin) : IRequest<Result>;

public class DeleteCategoryHandler : IRequestHandler<DeleteCategoryCommand, Result>
{
    private readonly AppDbContext _dbContext;
    public DeleteCategoryHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Coordinates retrieval, mapping and deletion of a specific category.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<Result> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        if (!request.IsAdmin)
        {
            return Result.Failure(Error.Forbidden);
        }
        var categoryResult = await GetCategoryAsync(request.CategoryId, cancellationToken);
        if (categoryResult.IsFailure)
        {
            return Result.Failure<ReadCategoryDto>(categoryResult.Error);
        }
        await DeleteCategoryAsync(categoryResult.Value, cancellationToken);
        return Result.Success();
    }

    /// <summary>
    /// Loads a product object based on the CategoryId.
    /// </summary>
    /// <param name="categoryId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Result<Category>> GetCategoryAsync(Guid categoryId, CancellationToken cancellationToken)
    {
        var category = await _dbContext.Categories
            .FirstOrDefaultAsync(c => c.CategoryId == categoryId, cancellationToken);
        if (category is null)
        {
            return Result.Failure<Category>(Error.NotFound);
        }
        return Result.Success(category);
    }

    /// <summary>
    /// Deletes the specified category persisting the changes to the database table. 
    /// </summary>
    /// <param name="category"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task DeleteCategoryAsync(Category category, CancellationToken cancellationToken)
    {
        _dbContext.Categories.Remove(category);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
