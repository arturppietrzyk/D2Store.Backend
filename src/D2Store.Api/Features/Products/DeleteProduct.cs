using D2Store.Api.Features.Products.Domain;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Products;

public record DeleteProductCommand(Guid ProductId, bool IsAdmin) : IRequest<Result>;

public class DeleteProductHandler : IRequestHandler<DeleteProductCommand, Result>
{
    private readonly AppDbContext _dbContext;

    public DeleteProductHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Coordinates retrieval, mapping and deletion of a specific product.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
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
        var hasOrderProducts = await _dbContext.OrderProducts.AsNoTracking().AnyAsync(op => op.ProductId == request.ProductId, cancellationToken);
        var assertOrderProductExistanceResult = Product.AssertOrderProductExistance(hasOrderProducts);
        if (assertOrderProductExistanceResult.IsFailure)
        {
            return Result.Failure(assertOrderProductExistanceResult.Error);
        }
        await DeleteProductAsync(productResult.Value, cancellationToken);
        return Result.Success();
    }

    /// <summary>
    /// Loads a product object based on the ProductId.
    /// </summary>
    /// <param name="productId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Result<Product>> GetProductAsync(Guid productId, CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products
            .FirstOrDefaultAsync(p => p.ProductId == productId, cancellationToken);
        if(product is null)
        {
            return Result.Failure<Product>(Error.NotFound);
        }
        return Result.Success(product);
    }

    /// <summary>
    /// Deletes the specified product persisting the changes to the database table. 
    /// </summary>
    /// <param name="product"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task DeleteProductAsync(Product product, CancellationToken cancellationToken)
    {
        _dbContext.Products.Remove(product);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
