using D2Store.Api.Features.Products.Domain;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Products;

public record DeleteProductCommand(Guid ProductId) : IRequest<Result<Guid>>;

public class DeleteProductHandler : IRequestHandler<DeleteProductCommand, Result<Guid>>
{
    private readonly AppDbContext _dbContext;

    public DeleteProductHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Coordinates retrieval, mapping and deletion of a specific product. Returns the Guid of the deleted product if successful. 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<Result<Guid>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await GetProductAsync(request.ProductId, cancellationToken);
        if (product is null)
        {
            return ProductNotFoundResult();
        }
        var orderProductsExist = await _dbContext.OrderProducts.AsNoTracking().AnyAsync(op => op.ProductId == request.ProductId, cancellationToken);
        var deleteProductResult = await DeleteProductAsync(product, orderProductsExist, cancellationToken);
        return deleteProductResult;
    }

    /// <summary>
    /// Loads a product object based on the ProductId.
    /// </summary>
    /// <param name="productId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Product?> GetProductAsync(Guid productId, CancellationToken cancellationToken)
    {
        return await _dbContext.Products
            .FirstOrDefaultAsync(p => p.ProductId == productId, cancellationToken);
    }

    /// <summary>
    /// Creates a failure result response for when a specified product cannot be found.
    /// </summary>
    /// <returns></returns>
    private static Result<Guid> ProductNotFoundResult()
    {
        return Result.Failure<Guid>(new Error(
            "DeleteProduct.Validation",
            "The product with the specified Product Id was not found."));
    }

    /// <summary>
    /// Validates the business rules and deletes the specified product persisting the changes to the database table. 
    /// </summary>
    /// <param name="product"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Result<Guid>> DeleteProductAsync(Product product, bool orderProductsExist, CancellationToken cancellationToken)
    {
        var deleteProductResult = product.Delete(orderProductsExist);
        if (deleteProductResult.IsFailure)
        {
            return Result.Failure<Guid>(deleteProductResult.Error);
        }
        _dbContext.Products.Remove(product);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(product.ProductId);
    }
}
