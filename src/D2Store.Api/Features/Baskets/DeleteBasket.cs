using D2Store.Api.Features.Baskets.Domain;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Baskets;

public record DeleteBasketCommand(Guid BasketProductId, Guid AuthenticatedUserId, bool IsAdmin) : IRequest<Result>;

public class DeleteBasketHandler : IRequestHandler<DeleteBasketCommand, Result>
{
    private readonly AppDbContext _dbContext;

    public DeleteBasketHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask<Result> Handle(DeleteBasketCommand request, CancellationToken cancellationToken)
    {
        var basketResult = await GetBasketProductAsync(request.BasketProductId, cancellationToken);
        if (basketResult.IsFailure)
        {
            return Result.Failure(basketResult.Error);
        }
        var basket = await _dbContext.Baskets.FirstOrDefaultAsync(x => x.BasketId == basketResult.Value.BasketId);
        var userId = basket!.UserId;
        if (!request.IsAdmin && userId != request.AuthenticatedUserId)
        {
            return Result.Failure(Error.Forbidden);
        }
        await DeleteBasketAsync(basketResult.Value, cancellationToken);
        var basketProd1 = await _dbContext.BasketProducts.FirstOrDefaultAsync(x => x.BasketId == basketResult.Value.BasketId);
        if (basketProd1 is null)
        {
            _dbContext.Baskets.Remove(basket);
        }
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private async Task<Result<BasketProduct>> GetBasketProductAsync(Guid basketProductId, CancellationToken cancellationToken)
    {
        var basketProduct = await _dbContext.BasketProducts
        .FirstOrDefaultAsync(bp => bp.BasketProductId == basketProductId, cancellationToken);
        if (basketProduct is null)
        {
            return Result.Failure<BasketProduct>(Error.NotFound);
        }
        return Result.Success(basketProduct);
    }

    private async Task DeleteBasketAsync(BasketProduct basketProduct, CancellationToken cancellationToken)
    {
        if (basketProduct.Quantity >= 1)
        {
            basketProduct.UpdateQuantity(basketProduct.Quantity - 1);
        }
        if (basketProduct.Quantity == 0)
        {
            _dbContext.BasketProducts.Remove(basketProduct);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}