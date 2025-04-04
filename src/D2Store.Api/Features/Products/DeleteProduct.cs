using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Products;

public record DeleteProductCommand(Guid ProductId) : IRequest<Result<Guid>>;

public class DeleteProductHandler : IRequestHandler<DeleteProductCommand, Result<Guid>>
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<DeleteProductHandler> _logger;

    public DeleteProductHandler(AppDbContext dbContext, ILogger<DeleteProductHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async ValueTask<Result<Guid>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.ProductId == request.ProductId, cancellationToken);
        if (product == null)
        {
            var result = Result.Failure<Guid>(new Error("DeleteProduct.Validation", "Product not found."));
            _logger.LogWarning("{Class}: {Method} - Warning: {ErrorCode} - {ErrorMessage}.", nameof(DeleteProductHandler), nameof(Handle), result.Error.Code, result.Error.Message);
            return result;
        }
        _dbContext.Products.Remove(product);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("{Class}: {Method} - Success, deleted {orderId}.", nameof(DeleteProductHandler), nameof(Handle), product.ProductId);
        return Result.Success(product.ProductId);
    }
}
