using D2Store.Api.Features.Products.Domain;
using D2Store.Api.Features.Products.Dto;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Products;

public record GetProductByIdQuery(Guid ProductId) : IRequest<Result<ReadProductDto>>;

public class GetProductByIdHandler : IRequestHandler<GetProductByIdQuery, Result<ReadProductDto>>
{
    private readonly AppDbContext _dbContext;

    public GetProductByIdHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Coordinates retrieval and mapping of a specific product into a response DTO.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<Result<ReadProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await GetProductAsync(request.ProductId, cancellationToken);
        if(product is null)
        {
            return CreateProductNotFoundResult();
        }
        return Result.Success(MapToReadProductDto(product));
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
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProductId == productId, cancellationToken);
    }

    /// <summary>
    /// Creates a failure result response for when a specified product cannot be found.
    /// </summary>
    /// <returns></returns>
    private static Result<ReadProductDto> CreateProductNotFoundResult()
    {
        return Result.Failure<ReadProductDto>(new Error(
            "GetProductById.Validation",
            "The product with the specified Product Id was not found."));
    }

    /// <summary>
    /// Maps the retrieved product into the ReadProductDto which is returned as the response. 
    /// </summary>
    /// <param name="product"></param>
    /// <returns></returns>
    private static ReadProductDto MapToReadProductDto(Product product)
    {
        return new ReadProductDto(
            product.ProductId, 
            product.Name, 
            product.Description, 
            product.Price,
            product.StockQuantity, 
            product.AddedDate, 
            product.LastModified);
    }
}
