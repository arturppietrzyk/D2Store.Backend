using D2Store.Api.Features.Products.Domain;
using D2Store.Api.Features.Products.Dto;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using FluentValidation;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Products;

public record GetProductsQuery(int PageNumber, int PageSize) : IRequest<Result<IReadOnlyCollection<ReadProductDto>>>;

public class GetProductsHandler : IRequestHandler<GetProductsQuery, Result<IReadOnlyCollection<ReadProductDto>>>
{
    private readonly AppDbContext _dbContext;
    private readonly IValidator<GetProductsQuery> _validator;

    public GetProductsHandler(AppDbContext dbContext, IValidator<GetProductsQuery> vallidator)
    {
        _dbContext = dbContext;
        _validator = vallidator;
    }

    /// <summary>
    /// Coordinates validation, retrieval and mapping of the specific products into a collection of response DTOs.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<Result<IReadOnlyCollection<ReadProductDto>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var validationResult = await ValidateRequestAsync(request, cancellationToken);
        if (validationResult.IsFailure)
        {
            return Result.Failure<IReadOnlyCollection<ReadProductDto>>(validationResult.Error);
        }
        var products = await GetPaginatedProductsAsync(request.PageNumber, request.PageSize, cancellationToken);
        var productDtos = products.Select(MapToReadProductDto).ToList();
         return Result.Success<IReadOnlyCollection<ReadProductDto>>(productDtos);
    }

    /// <summary>
    /// Validates the PageNumber and PageSize Pagination parameters. 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Result> ValidateRequestAsync(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure<IReadOnlyCollection<ReadProductDto>>(new Error("GetProducts.Validation", validationResult.ToString()));
        }
        return Result.Success();
    }

    /// <summary>
    /// Loads the product objects along side its images, based on the Pagination parameters.
    /// </summary>
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<IReadOnlyCollection<Product>> GetPaginatedProductsAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        return await _dbContext.Products
            .Include(p => p.Images)
            .AsNoTracking()
            .OrderByDescending(p => p.AddedDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Maps an Product entity along side its images into a ReadProductDto. 
    /// </summary>
    /// <param name="product"></param>
    /// <returns></returns>
    private static ReadProductDto MapToReadProductDto(Product product)
    {
        var productImageDto = product.Images.Select(pi => new ReadProductImageDto(
            pi.ProductImageId,
            pi.Location,
            pi.IsPrimary
        )).ToList();
        return new ReadProductDto(
            product.ProductId,
            product.Name,
            product.Description,
            product.Price,
            product.StockQuantity,
            product.AddedDate,
            product.LastModified,
            productImageDto);
    }
}

public class GetProductsQueryValidator : AbstractValidator<GetProductsQuery>
{
    public GetProductsQueryValidator()
    {
        RuleFor(p => p.PageNumber).GreaterThan(0).WithMessage("Page Number must be greater than 0.");
        RuleFor(p => p.PageSize).GreaterThan(0).WithMessage("Page Size must be greater than 0.");
    }
}