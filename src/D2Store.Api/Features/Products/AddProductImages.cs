using D2Store.Api.Features.Products.Domain;
using D2Store.Api.Features.Products.Dto;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using FluentValidation;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Products;

public record AddProductImagesCommand(Guid ProductId, List<WriteProductImageDtoCreate> Images, bool IsAdmin) : IRequest<Result>;

public class AddProductImagesHandler : IRequestHandler<AddProductImagesCommand, Result>
{
    private readonly AppDbContext _dbContext;
    private readonly IValidator<AddProductImagesCommand> _validator;

    public AddProductImagesHandler(AppDbContext dbContext, IValidator<AddProductImagesCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

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

    private async Task<Result> ValidateRequestAsync(AddProductImagesCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure(new Error("AddProductImages.Validation", validationResult.ToString()));
        }
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
        RuleFor(request => request.Images).NotEmpty().WithMessage("At least one image is required.");
        RuleForEach(request => request.Images).ChildRules(images =>
        {
            images.RuleFor(img => img.Location).NotEmpty().WithMessage("location value is required");
            images.RuleFor(img => img.Location).NotEmpty().WithMessage("IsPrimary value is required.");
        });
    }
}