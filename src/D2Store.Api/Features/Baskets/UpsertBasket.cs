using D2Store.Api.Features.Baskets.Domain;
using D2Store.Api.Features.Baskets.Dto;
using D2Store.Api.Features.Products.Domain;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using FluentValidation;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Baskets;

public record UpsertBasketCommand(Guid UserId, WriteBasketProductDtoCreate Product, Guid AuthenticatedUserId, bool isAdmin) : IRequest<Result<ReadBasketDto>>;

public class UpsertBasketHandler : IRequestHandler<UpsertBasketCommand, Result<ReadBasketDto>>
{
    private readonly AppDbContext _dbContext;
    private readonly IValidator<UpsertBasketCommand> _validator;

    public UpsertBasketHandler(AppDbContext dbContext, IValidator<UpsertBasketCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async ValueTask<Result<ReadBasketDto>> Handle(UpsertBasketCommand request, CancellationToken cancellationToken)
    {
        if (!request.isAdmin && request.UserId != request.AuthenticatedUserId)
        {
            return Result.Failure<ReadBasketDto>(Error.Forbidden);
        }
        var requestValidationResult = await ValidateRequestAsync(request, cancellationToken);
        if (requestValidationResult.IsFailure)
        {
            return Result.Failure<ReadBasketDto>(requestValidationResult.Error);
        }
        var userExists = await _dbContext.Users.AsNoTracking().AnyAsync(u => u.UserId == request.UserId, cancellationToken);
        var assertUserExistanceResult = Basket.AssertUserExsistance(userExists);
        if (assertUserExistanceResult.IsFailure)
        {
            return Result.Failure<ReadBasketDto>(assertUserExistanceResult.Error);
        }
        var productExists = await _dbContext.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.ProductId == request.Product.ProductId, cancellationToken);
        var assertProductExistanceResult = Basket.AssertProductsExistance(productExists is not null);
        if (assertProductExistanceResult.IsFailure)
        {
            return Result.Failure<ReadBasketDto>(assertProductExistanceResult.Error);
        }
        var assertStockAvailabilityResult = Basket.AssertStockAvailability(request, productExists!);
        if(assertStockAvailabilityResult.IsFailure)
        {
            return Result.Failure<ReadBasketDto>(assertStockAvailabilityResult.Error);
        }
        var upsertBasket = await UpsertBasketAsync(request, productExists!, cancellationToken);
        var productsDto = MapOrderProductsToDto(upsertBasket.Products.ToList());
        var basketDto = MapToReadBasketDto(upsertBasket, productsDto);
        return Result.Success(basketDto);
    }

    private async Task<Result> ValidateRequestAsync(UpsertBasketCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure(new Error("UpsertBasket.Validation", validationResult.ToString()));
        }
        return Result.Success();
    }

    private async Task<Basket> UpsertBasketAsync(UpsertBasketCommand request, Product product, CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var basket = await _dbContext.Baskets
            .Include(b => b.Products.Where(p => p.ProductId == request.Product.ProductId))
            .FirstOrDefaultAsync(b => b.UserId == request.UserId, cancellationToken);
            if (basket is null)
            {
                basket = Basket.Create(request.UserId);
                _dbContext.Baskets.Add(basket);
            }
            var existingBasketProduct = basket.Products.FirstOrDefault(b => b.ProductId == request.Product.ProductId);
            if (existingBasketProduct is not null)
            {
                existingBasketProduct.UpdateQuantity(existingBasketProduct.Quantity + request.Product.Quantity);
                basket.UpdateExistingProductQuantity(product, request.Product.Quantity);
            }
            else
            {
                basket.AddProduct(product, request.Product.Quantity);
            }
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return basket;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private IReadOnlyCollection<ReadBasketProductDto> MapOrderProductsToDto(IReadOnlyCollection<BasketProduct> basketProducts)
    {
        return basketProducts.Select(op =>
        {
            return new ReadBasketProductDto(
                op.Product.ProductId,
                op.Product.Name,
                op.Product.Description,
                op.Product.Price,
                op.Quantity,
                op.Product.Images.First(i => i.IsPrimary).ProductImageId,
                op.Product.Images.First(i => i.IsPrimary).Location
            );
        }).ToList();
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

public class UpsertBasketCommandValidator : AbstractValidator<UpsertBasketCommand>
{
    public UpsertBasketCommandValidator()
    {
        RuleFor(c => c.Product).NotEmpty().WithMessage("At least one product must be provided.")
            .Must(product => product.Quantity > 0).WithMessage("Product must have a quantity greater than 0.");
    }
}
