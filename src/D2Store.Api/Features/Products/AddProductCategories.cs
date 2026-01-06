using D2Store.Api.Features.Products.Domain;
using D2Store.Api.Features.Products.Dto;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Products;

public record AddProductCategoriesCommand(Guid ProductId, IReadOnlyCollection<WriteProductCategoryDtoCreate> Categories, bool IsAdmin) : IRequest<Result>;

public class AddProductCategoriesHandler : IRequestHandler<AddProductCategoriesCommand, Result>
{
    private readonly AppDbContext _dbContext;

    public AddProductCategoriesHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Coordinates, retrieval and the addition of new categories to an existing product.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<Result> Handle(AddProductCategoriesCommand request, CancellationToken cancellationToken)
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
        var incomingIds = request.Categories.Select(x => x.CategoryId);
        var assertProductCategoriesDoNotExist = productResult.Value.AssertProductCategoriesDoNotExist(incomingIds);
        if(assertProductCategoriesDoNotExist.IsFailure)
        {
            return Result.Failure(assertProductCategoriesDoNotExist.Error);
        }
        await AddProductCategoriesAsync(request, productResult.Value, cancellationToken);
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
    /// Adds the new product categories to an existing product and presists the changes to the db tables.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="product"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task AddProductCategoriesAsync(AddProductCategoriesCommand request, Product product, CancellationToken cancellationToken)
    {
        foreach(var prodCat in request.Categories)
        {
            product.AddCategory(prodCat.CategoryId);
        }
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}