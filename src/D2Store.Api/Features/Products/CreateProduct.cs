using D2Store.Api.Features.Products.Domain;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using FluentValidation;
using Mediator;
using D2Store.Api.Features.Products.Dto;
using System.Data;

namespace D2Store.Api.Features.Products;

public record CreateProductCommand(string Name, string Description, decimal Price, int StockQuantity, IReadOnlyCollection<WriteProductImageDtoCreate> ImagesDto, bool IsAdmin) : IRequest<Result<ReadProductDto>>;

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
    /// Coordinates validation, mapping and creating of an product. Returns the created product in a form of a response DTO. 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<Result<ReadProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        if (!request.IsAdmin)
        {
            return Result.Failure<ReadProductDto>(Error.Forbidden);
        }
        var validationResult = await ValidateRequestAsync(request, cancellationToken);
        if (validationResult.IsFailure)
        {
            return Result.Failure<ReadProductDto>(validationResult.Error);
        }
        var createProduct = await CreateProductAsync(request, cancellationToken);
        var productImageDtos = MapProductImagesToDto(createProduct.Images.ToList());
        var productDto = MapToReadProductDto(createProduct, productImageDtos);
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
    private async Task<Product> CreateProductAsync(CreateProductCommand request, CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var product = Product.Create(request.Name, request.Description, request.Price, request.StockQuantity);
            _dbContext.Products.Add(product);
            foreach (var prodImg in request.ImagesDto)
            {
                product.AddImage(prodImg.Location, prodImg.IsPrimary);
            }
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return product;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Maps the collection of product images into the equivalent ReadProductImageDto collection. 
    /// </summary>
    /// <param name="productImages"></param>
    /// <returns></returns>
    private IReadOnlyCollection<ReadProductImageDto> MapProductImagesToDto(IReadOnlyCollection<ProductImage> productImages)
    {
        return productImages.Select(pi => new ReadProductImageDto(
            pi.ProductImageId,
            pi.Location,
            pi.IsPrimary
        )).ToList();
    }

    /// <summary>
    /// Maps the retrieved product into the ReadProductDto which is returned as the response. 
    /// </summary>
    /// <param name="product"></param>
    /// <returns></returns>
    private static ReadProductDto MapToReadProductDto(Product product, IReadOnlyCollection<ReadProductImageDto> images)
    {
        return new ReadProductDto(
            product.ProductId,
            product.Name,
            product.Description,
            product.Price,
            product.StockQuantity,
            product.AddedDate,
            product.LastModified,
            images);
    }
}

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(p => p.Name).NotNull().NotEmpty().WithMessage("Name is required.");
        RuleFor(p => p.Description).NotNull().NotEmpty().WithMessage("Description is required.");
        RuleFor(p => p.Price).NotNull().GreaterThan(0).WithMessage("Price is required and must be greater than zero.");
        RuleFor(p => p.StockQuantity).NotNull().GreaterThan(0).WithMessage("Stock Quantity is required and must be greater than zero.");
        RuleFor(p => p.ImagesDto).NotNull().NotEmpty().WithMessage("At least one image must be present.")
            .Must(images => images.Any()).WithMessage("At least one image must be provided.")
            .Must(images => images.All(img => !string.IsNullOrEmpty(img.Location))).WithMessage("All images must have a location specified.")
            .Must(images => images.Count(img => img.IsPrimary == true) == 1).WithMessage("One image must be set as primary.");
    }
}
