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

    public async ValueTask<Result<Guid>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.ProductId == request.ProductId, cancellationToken);
        if (product == null)
        {
            var result = Result.Failure<Guid>(new Error("DeleteProduct.Validation", "Product not found."));
            return result;
        }
        _dbContext.Products.Remove(product);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(product.ProductId);
    }
}
