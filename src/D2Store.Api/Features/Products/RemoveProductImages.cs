using D2Store.Api.Features.Products.Domain;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using FluentValidation;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Products;

public record RemoveProductImagesCommand(Guid ProductId, List<Guid> ProductImageIds, bool IsAdmin) : IRequest<Result>;


public class RemoveProductImagesHandler : IRequestHandler<RemoveProductImagesCommand, Result>
{
    private readonly AppDbContext _dbContext;

    public RemoveProductImagesHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask<Result> Handle(RemoveProductImagesCommand request, CancellationToken cancellationToken)
    {
        if (!request.IsAdmin)
        {
            return Result.Failure(Error.Forbidden);
        }
        var productResult = await GetProductAsync(request.ProductId, cancellationToken);
        if (productResult.IsFailure)
        {
            return Result.Failure(productResult.Error);
        }
        await RemoveProductImagesAsync(request, productResult.Value, cancellationToken);
        return Result.Success();
    }

    private async Task<Result<Product>> GetProductAsync(Guid productId, CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.ProductId == productId, cancellationToken);
        if (product is null)
        {
            return Result.Failure<Product>(Error.NotFound);
        }
        return Result.Success(product);
    }

    private async Task RemoveProductImagesAsync(RemoveProductImagesCommand request, Product product, CancellationToken cancellationToken)
    {
        product.RemoveImages(request.ProductImageIds);
        await _dbContext.SaveChangesAsync();
    }
}

public class RemoveProductImagesCommandValidator : AbstractValidator<RemoveProductImagesCommand>
{
    public RemoveProductImagesCommandValidator()
    {
        RuleFor(request => request.ProductImageIds).NotNull().NotEmpty().WithMessage("At least one productId must be provided");
    }
}