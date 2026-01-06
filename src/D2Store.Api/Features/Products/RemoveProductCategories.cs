using D2Store.Api.Features.Products.Domain;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Products;

public record RemoveProductCategoriesCommand(Guid ProductId, IReadOnlyCollection<Guid> ProductCategoryIds, bool IsAdmin) : IRequest<Result>;

public class RemoveProductCategoriesHandler : IRequestHandler<RemoveProductCategoriesCommand, Result>
{
    private readonly AppDbContext _dbContext;

    public RemoveProductCategoriesHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Coordinates the retrieval and the removal of categories of an existing product.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<Result> Handle(RemoveProductCategoriesCommand request, CancellationToken cancellationToken)
    {
        if (!request.IsAdmin)
        {
            return Result.Failure(Error.Forbidden);
        }
        var productResult = await GetProductAsync(request.ProductId, cancellationToken);
        if (productResult.IsFailure)
        {
            return Result.Failure(productResult.Error);
        }
        await RemoveProductCategoriesAsync(request, productResult.Value, cancellationToken);
        return Result.Success();
    }

    /// <summary>
    /// Loads a product object along with its categories, based on the ProductId.
    /// </summary>
    /// <param name="productId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Result<Product>> GetProductAsync(Guid productId, CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products
            .Include(p => p.Categories)
            .FirstOrDefaultAsync(p => p.ProductId == productId, cancellationToken);
        if (product is null)
        {
            return Result.Failure<Product>(Error.NotFound);
        }
        return Result.Success(product);
    }

    /// <summary>
    /// Removes existing product categories of an existing product and presists the changes to the database. 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="product"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task RemoveProductCategoriesAsync(RemoveProductCategoriesCommand request, Product product, CancellationToken cancellationToken)
    {
        product.RemoveCategories(request.ProductCategoryIds);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}