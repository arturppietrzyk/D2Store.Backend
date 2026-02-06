using D2Store.Api.Features.Baskets.Domain;
using D2Store.Api.Features.Baskets.Dto;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Baskets;

public record GetBasketQuery(Guid BasketId, Guid AuthenticatedUserId, bool IsAdmin) : IRequest<Result<ReadBasketDto>>;

public class GetBasketHandler : IRequestHandler<GetBasketQuery, Result<ReadBasketDto>>
{
    private readonly AppDbContext _dbContext;

    public GetBasketHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask<Result<ReadBasketDto>> Handle(GetBasketQuery request, CancellationToken cancellationToken)
    {
        var basketResult = await GetBasketAsync(request.BasketId, cancellationToken);
        if (basketResult.IsFailure)
        {
            return Result.Failure<ReadBasketDto>(basketResult.Error);
        }
        if (!request.IsAdmin && basketResult.Value.UserId != request.AuthenticatedUserId)
        {
            return Result.Failure<ReadBasketDto>(Error.Forbidden);
        }
        var orderProductsDto = MapBasketProductsToDto(basketResult.Value.Products);
        return Result.Success(MapToReadBasketDto(basketResult.Value, orderProductsDto));
    }

    private async Task<Result<Basket>> GetBasketAsync(Guid basketId, CancellationToken cancellationToken)
    {
        var basketExists = await _dbContext.Baskets
         .AsNoTracking()
         .Include(o => o.Products)
         .ThenInclude(bp => bp.Product)
         .ThenInclude(p => p.Images)
         .FirstOrDefaultAsync(b => b.BasketId == basketId, cancellationToken);
        if (basketExists is null)
        {
            return Result.Failure<Basket>(Error.NotFound);
        }
        return Result.Success(basketExists);
    }

    private IReadOnlyCollection<ReadBasketProductDto> MapBasketProductsToDto(IReadOnlyCollection<BasketProduct> basketProducts)
    {
        return basketProducts.Select(op => new ReadBasketProductDto(
            op.Product.ProductId,
            op.Product.Name,
            op.Product.Description,
            op.Product.Price,
            op.Quantity,
            op.Product.Images.First(i => i.IsPrimary).ProductImageId,
            op.Product.Images.First(i => i.IsPrimary).Location
        )).ToList();
    }

    private static ReadBasketDto MapToReadBasketDto(Basket basket, IReadOnlyCollection<ReadBasketProductDto> products)
    {
        return new ReadBasketDto(
            basket.BasketId,
            basket.UserId,
            products,
            basket.CreatedAt,
            basket.TotalAmount,
            basket.LastModified);
    }
}