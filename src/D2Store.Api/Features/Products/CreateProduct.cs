using D2Store.Api.Features.Products.Domain;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using FluentValidation;
using Mediator;
using D2Store.Api.Features.Products.Dto;

namespace D2Store.Api.Features.Products;

public record CreateProductCommand(string Name, string Description, decimal Price, int StockQuantity) : IRequest<Result<ReadProductDto>>;

public class CreateProductHandler : IRequestHandler<CreateProductCommand, Result<ReadProductDto>>
{
    private readonly AppDbContext _dbContext;
    private readonly IValidator<CreateProductCommand> _validator;

    public CreateProductHandler(AppDbContext dbContext, IValidator<CreateProductCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    /// <summary>
    /// Coordinates validation, mapping and creating of an product. Returns the created product in a response DTO.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<Result<ReadProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await ValidateRequestAsync(request, cancellationToken);
        if (validationResult.IsFailure)
        {
            return Result.Failure<ReadProductDto>(validationResult.Error);
        }
        var createProductResult = await CreateProductAsync(request, cancellationToken);
        var productDto = MapToReadProductDto(createProductResult.Value);
        return Result.Success(productDto); 
    }

    /// <summary>
    /// Validates the input. 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Result> ValidateRequestAsync(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure<Product>(new Error("CreateProduct.Validation", validationResult.ToString()));
        }
        return Result.Success();
    }

    /// <summary>
    /// Creates the product and persists it to the database. 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Result<Product>> CreateProductAsync(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var createProductResult = Product.Create(request.Name, request.Description, request.Price, request.StockQuantity);
        _dbContext.Products.Add(createProductResult.Value);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return createProductResult;
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

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(p => p.Name).NotEmpty().WithMessage("Name is required.");
        RuleFor(p => p.Description).NotEmpty().WithMessage("Description is required.");
        RuleFor(p => p.Price).GreaterThan(0).WithMessage("Price must be greater than zero.");
        RuleFor(p => p.StockQuantity).GreaterThan(0).WithMessage("Stock Quantity must be greater than zero.");
    }
}