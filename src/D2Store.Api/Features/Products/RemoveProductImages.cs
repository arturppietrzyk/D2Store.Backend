using D2Store.Api.Features.Products.Domain;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using FluentValidation;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Products;

public record RemoveProductImagesCommand(Guid ProductId, IReadOnlyCollection<Guid> ProductImageIds, bool IsAdmin) : IRequest<Result>;

public class RemoveProductImagesHandler : IRequestHandler<RemoveProductImagesCommand, Result>
{
    private readonly AppDbContext _dbContext;
    private IValidator<RemoveProductImagesCommand> _validator;

    public RemoveProductImagesHandler(AppDbContext dbContext, IValidator<RemoveProductImagesCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    /// <summary>
    /// Coordinates validation, retrieval and the removal of new images of an existing product.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<Result> Handle(RemoveProductImagesCommand request, CancellationToken cancellationToken)
    {
        if (!request.IsAdmin)
        {
            return Result.Failure(Error.Forbidden);
        }
        var validationResult = await ValidateRequestAsync(request, cancellationToken);
        if (validationResult.IsFailure)
        {
            return Result.Failure(validationResult.Error);
        }
        var productResult = await GetProductAsync(request.ProductId, cancellationToken);
        if (productResult.IsFailure)
        {
            return Result.Failure(productResult.Error);
        }
        var assertProductImageBeingRemovedIsNotAPrimaryImageResult = productResult.Value.AssertProductImageBeingRemovedIsNotAPrimaryImage(request.ProductImageIds);
        if (assertProductImageBeingRemovedIsNotAPrimaryImageResult.IsFailure)
        {
            return Result.Failure(assertProductImageBeingRemovedIsNotAPrimaryImageResult.Error);
        }
        await RemoveProductImagesAsync(request, productResult.Value, cancellationToken);
        return Result.Success();
    }

    /// <summary>
    /// Validates the input.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Result> ValidateRequestAsync(RemoveProductImagesCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure(new Error("RemoveProductImages.Validation", validationResult.ToString()));
        }
        return Result.Success();
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
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.ProductId == productId, cancellationToken);
        if (product is null)
        {
            return Result.Failure<Product>(Error.NotFound);
        }
        return Result.Success(product);
    }

    /// <summary>
    /// Removes existing product images of an existing product and presists the changes to the database. 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="product"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
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
        RuleFor(request => request.ProductImageIds).NotNull().NotEmpty().WithMessage("At least one productId must be provided.");
    }
}