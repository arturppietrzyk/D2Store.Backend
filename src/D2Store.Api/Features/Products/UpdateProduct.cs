using D2Store.Api.Features.Products.Domain;
using D2Store.Api.Features.Products.Dto;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using FluentValidation;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Products;

public record UpdateProductCommand(Guid ProductId, string? Name, string? Description, decimal? Price, int? StockQuantity) : IRequest<Result<ReadProductDto>>;

public class UpdateProductHandler : IRequestHandler<UpdateProductCommand, Result<ReadProductDto>>
{
    private readonly AppDbContext _dbContext;
    private readonly IValidator<UpdateProductCommand> _validator;

    public UpdateProductHandler(AppDbContext dbContext, IValidator<UpdateProductCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    /// <summary>
    /// Coordinates validation, retrieval, mapping and updating of a specific product. Returns the updated product in a response DTO.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<Result<ReadProductDto>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await ValidateRequestAsync(request, cancellationToken);
        if (validationResult.IsFailure)
        {
            return Result.Failure<ReadProductDto>(validationResult.Error);
        }
        var product = await GetProductAsync(request.ProductId, cancellationToken);
        if (product is null)
        {
            return CreateProductNotFoundResult();
        }
        await UpdateProductAsync(product, request, cancellationToken);
        return Result.Success(MapToReadProductDto(product));
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
            return Result.Failure<ReadProductDto>(new Error("UpdateProduct.Validation", validationResult.ToString()));
        }
        return Result.Success();
    }

    /// <summary>
    /// Loads a product object based on the ProductId.
    /// </summary>
    /// <param name="productId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Product?> GetProductAsync(Guid productId, CancellationToken cancellationToken)
    {
        return await _dbContext.Products
            .FirstOrDefaultAsync(p => p.ProductId == productId, cancellationToken);
    }

    /// <summary>
    /// Creates a failure result response for when a specified product cannot be found.
    /// </summary>
    /// <returns></returns>
    private static Result<ReadProductDto> CreateProductNotFoundResult()
    {
        return Result.Failure<ReadProductDto>(new Error(
            "UpdateProduct.Validation",
            "The product with the specified Product Id was not found."));
    }

    /// <summary>
    /// Updates the product and persists the changes in the database table. 
    /// </summary>
    /// <param name="product"></param>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Result<Product>> UpdateProductAsync(Product product, UpdateProductCommand request, CancellationToken cancellationToken)
    {
        product.UpdateProductInfo(request.Name, request.Description, request.Price, request.StockQuantity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(product);
    }

    /// <summary>
    /// Maps the retrieved product into the ReadProductDto which is returned as the response. 
    /// </summary>
    /// <param name="product"></param>
    /// <returns></returns>
    private static ReadProductDto MapToReadProductDto(Product product)
    {
        return new ReadProductDto(
            product.ProductId,
            product.Name,
            product.Description,
            product.Price,
            product.StockQuantity,
            product.AddedDate,
            product.LastModified);
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