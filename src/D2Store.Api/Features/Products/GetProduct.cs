using D2Store.Api.Features.Products.Domain;
using D2Store.Api.Features.Products.Dto;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Products;

public record GetProductQuery(Guid ProductId) : IRequest<Result<ReadProductDto>>;

public class GetProductHandler : IRequestHandler<GetProductQuery, Result<ReadProductDto>>
{
    private readonly AppDbContext _dbContext;

    public GetProductHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Coordinates retrieval and mapping of a specific product into a response DTO.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<Result<ReadProductDto>> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        var productResult = await GetProductAsync(request.ProductId, cancellationToken);
        if (productResult.IsFailure)
        {
            return Result.Failure<ReadProductDto>(productResult.Error);
        }
        var productImageDtos = MapProductImagesToDto(productResult.Value.Images);
        return Result.Success(MapToReadProductDto(productResult.Value, productImageDtos));
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
            .AsNoTracking()
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.ProductId == productId, cancellationToken);
        if (product is null)
        {
            return Result.Failure<Product>(Error.NotFound);
        }
        return Result.Success(product);
    }
    
    /// <summary>
    /// Maps the collection of product images into the equivalent ReadProductImageDto collection. 
    /// </summary>
    /// <param name="productImages"></param>
    /// <returns></returns>
    private IReadOnlyCollection<ReadProductImageDto> MapProductImagesToDto(IReadOnlyCollection<ProductImage> productImages)
    {
        return productImages.Select(pi => new ReadProductImageDto(
            pi.ProductImageId,
            pi.Location,
            pi.IsPrimary
        )).ToList();
    }

    /// <summary>
    /// Maps the retrieved product into the ReadProductDto which is returned as the response. 
    /// </summary>
    /// <param name="product"></param>
    /// <returns></returns>
    private static ReadProductDto MapToReadProductDto(Product product, IReadOnlyCollection<ReadProductImageDto> productImagesDto)
    {
        return new ReadProductDto(
            product.ProductId,
            product.Name,
            product.Description,
            product.Price,
            product.StockQuantity,
            product.AddedDate,
            product.LastModified,
            productImagesDto.ToList());
    }
}
