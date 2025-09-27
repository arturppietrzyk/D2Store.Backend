using D2Store.Api.Features.Products.Domain;
using D2Store.Api.Features.Products.Dto;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using FluentValidation;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Products;

public record AddProductImagesCommand(Guid ProductId, IReadOnlyCollection<WriteProductImageDtoCreate> Images, bool IsAdmin) : IRequest<Result>;

public class AddProductImagesHandler : IRequestHandler<AddProductImagesCommand, Result>
{
    private readonly AppDbContext _dbContext;
    private readonly IValidator<AddProductImagesCommand> _validator;

    public AddProductImagesHandler(AppDbContext dbContext, IValidator<AddProductImagesCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    /// <summary>
    /// Coordinates validation, retrieval and the addition of new images to an existing product.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<Result> Handle(AddProductImagesCommand request, CancellationToken cancellationToken)
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
        await AddProductImagesAsync(request, productResult.Value, cancellationToken);
        return Result.Success();
    }

    /// <summary>
    /// Validates the input.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Result> ValidateRequestAsync(AddProductImagesCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure(new Error("AddProductImages.Validation", validationResult.ToString()));
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
    /// Adds the new product images to an existing product and presists the changes to the db tables.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="product"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task AddProductImagesAsync(AddProductImagesCommand request, Product product, CancellationToken cancellationToken)
    {
        foreach (var prodImg in request.Images)
        {
            product.AddImage(prodImg.Location, prodImg.IsPrimary);
        }
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

public class AddProductImagesCommandValidator : AbstractValidator<AddProductImagesCommand>
{
    public AddProductImagesCommandValidator()
    {
        RuleFor(request => request.Images).NotNull().NotEmpty().WithMessage("At least one image is required.");
        RuleFor(request => request.Images).Must(images => images.All(img => !string.IsNullOrEmpty(img.Location))).WithMessage("All images must have a location specified.");
        RuleFor(request => request.Images).Must(images => images.All(img => img.IsPrimary == false)).WithMessage("A primary image cannot be added through this endpoint.");
    }
}