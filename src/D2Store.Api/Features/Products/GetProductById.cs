using D2Store.Api.Features.Orders;
using D2Store.Api.Features.Products.Dto;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Products;

public record GetProductByIdQuery(Guid ProductId) : IRequest<Result<ReadProductDto>>;

public class GetProductByIdHandler : IRequestHandler<GetProductByIdQuery, Result<ReadProductDto>>
{

    private readonly AppDbContext _dbContext;
    private readonly ILogger<GetProductByIdHandler> _logger;

    public GetProductByIdHandler(AppDbContext dbContext, ILogger<GetProductByIdHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<ReadProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == request.ProductId, cancellationToken);
        if (product == null)
        {
            var result = Result.Failure<ReadProductDto>(new Error("GetProductById.Validation", "The product with the specified Product Id was not found."));
            _logger.LogWarning("{Class}: {Method} - Warning: {ErrorCode} - {ErrorMessage}", nameof(GetProductByIdHandler), nameof(Handle), result.Error.Code, result.Error.Message);
            return result;
        }
        var productDto = new ReadProductDto(product.ProductId, product.Name, product.Description, product.Price, product.StockQuantity, product.AddedDate, product.LastModified);
        _logger.LogInformation("{Class}: {Method} - Success, retrieved: {orderId}.", nameof(GetOrdersHandler), nameof(Handle), productDto.ProductId.ToString());
        return Result.Success<ReadProductDto>(productDto);
    }
}
