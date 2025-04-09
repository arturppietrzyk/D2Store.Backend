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

    public async ValueTask<Result<ReadProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == request.ProductId, cancellationToken);
        if (product == null)
        {
            var result = Result.Failure<ReadProductDto>(new Error("GetProductById.Validation", "The product with the specified Product Id was not found."));
            return result;
        }
        var productDto = new ReadProductDto(product.ProductId, product.Name, product.Description, product.Price, product.StockQuantity, product.AddedDate, product.LastModified);
        return Result.Success<ReadProductDto>(productDto);
    }
}
