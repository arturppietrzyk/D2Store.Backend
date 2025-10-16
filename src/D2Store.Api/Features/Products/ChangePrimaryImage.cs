using D2Store.Api.Features.Products.Domain;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Products;

public record ChangePrimaryImageCommand(Guid ProductId, Guid ProductImageId, bool IsAdmin) : IRequest<Result>;

public class ChangePrimaryImageHandler : IRequestHandler<ChangePrimaryImageCommand, Result>
{
    private readonly AppDbContext _dbContext;

    public ChangePrimaryImageHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Coordinates the retrieval of a product to change the primary image. 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<Result> Handle(ChangePrimaryImageCommand request, CancellationToken cancellationToken)
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
        var isPrimaryImageChanged = await ChangePrimaryImageAsync(request, productResult.Value, cancellationToken);
        if (isPrimaryImageChanged.IsFailure)
        {
            return Result.Failure(isPrimaryImageChanged.Error);
        }
        return Result.Success();
    }

    /// <summary>
    /// Loads a product object along with its images, based on the ProductId.
    /// </summary>
    /// <param name="productId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Result<Product>> GetProductAsync(Guid productId, CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products
        .Include(p => p.Images)
        .FirstOrDefaultAsync(p => p.ProductId == productId);
        if (product is null)
        {
            return Result.Failure<Product>(Error.NotFound);
        }
        return Result.Success(product);
    }
    
    /// <summary>
    /// Changes the primary image to the image with the specified ProductImageId. If the image is already a primary image, the change does not happen and an error result is thrown. Changes are persisted to the database table.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="product"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Result> ChangePrimaryImageAsync(ChangePrimaryImageCommand request, Product product, CancellationToken cancellationToken)
    {
        var isPrimaryImageChanged = product.ChangePrimaryImage(request.ProductImageId);
        if (isPrimaryImageChanged.IsFailure)
        {
            return Result.Failure(isPrimaryImageChanged.Error);
        }
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}