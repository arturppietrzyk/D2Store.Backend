using D2Store.Api.Features.Products.Domain;
using D2Store.Api.Features.Products.Dto;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using FluentValidation;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Products;

public record UpdateProductCommand(Guid ProductId, string? Name, string? Description, decimal? Price, int? StockQuantity, bool isAdmin) : IRequest<Result<Guid>>;

public class UpdateProductHandler : IRequestHandler<UpdateProductCommand, Result<Guid>>
{
    private readonly AppDbContext _dbContext;
    private readonly IValidator<UpdateProductCommand> _validator;

    public UpdateProductHandler(AppDbContext dbContext, IValidator<UpdateProductCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    /// <summary>
    /// Coordinates validation, retrieval, mapping and updating of a specific product. Returns the Guid of the deleted product if successful. 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<Result<Guid>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        if (!request.isAdmin)
        {
            return Result.Failure<Guid>(Error.Forbidden);
        }
        var validationResult = await ValidateRequestAsync(request, cancellationToken);
        if (validationResult.IsFailure)
        {
            return Result.Failure<Guid>(validationResult.Error);
        }
        var productResult = await GetProductAsync(request.ProductId, cancellationToken);
        if (productResult.IsFailure)
        {
            return Result.Failure<Guid>(productResult.Error);
        }
        var updateProduct = await UpdateProductAsync(productResult.Value, request, cancellationToken);
        return Result.Success(updateProduct);
    }

    /// <summary>
    /// Validates the input. 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Result> ValidateRequestAsync(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure<Guid>(new Error("UpdateProduct.Validation", validationResult.ToString()));
        }
        return Result.Success();
    }

    /// <summary>
    /// Loads a product object based on the ProductId.
    /// </summary>
    /// <param name="productId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Result<Product>> GetProductAsync(Guid productId, CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products
            .FirstOrDefaultAsync(p => p.ProductId == productId, cancellationToken);
        if (product is null)
        {
            return Result.Failure<Product>(new Error(
            "UpdateProduct.Validation",
            "The product with the specified Product Id was not found."));
        }
        return Result.Success(product);
    }

    /// <summary>
    /// Updates the product and persists the changes in the database table. 
    /// </summary>
    /// <param name="product"></param>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Guid> UpdateProductAsync(Product product, UpdateProductCommand request, CancellationToken cancellationToken)
    {
        product.Update(request.Name, request.Description, request.Price, request.StockQuantity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return product.ProductId;
    }
}

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(p => p.Name).NotEmpty().When(p => p.Name is not null).WithMessage("Name cannot be empty if provided.");
        RuleFor(p => p.Description).NotEmpty().When(p => p.Description is not null).WithMessage("Description cannot be empty if provided.");
        RuleFor(p => p.Price).GreaterThan(0).When(p => p.Price is not null).WithMessage("Price cannot be 0 if provided.");
        RuleFor(p => p.StockQuantity).GreaterThan(-1).When(p => p.StockQuantity is not null).WithMessage("Stock Quantity cannot be negative if provided.");
    }
}