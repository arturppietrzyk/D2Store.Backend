using D2Store.Api.Features.Products.Dto;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using FluentValidation;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Products;

public record GetProductsQuery(int PageNumber, int PageSize) : IRequest<Result<List<ReadProductDto>>>;

public class GetProductsHandler : IRequestHandler<GetProductsQuery, Result<List<ReadProductDto>>>
{
    private readonly AppDbContext _dbContext;
    private readonly IValidator<GetProductsQuery> _validator;
    private readonly ILogger<GetProductsHandler> _logger;

    public GetProductsHandler(AppDbContext dbContext, IValidator<GetProductsQuery> vallidator, ILogger<GetProductsHandler> logger)
    {
        _dbContext = dbContext;
        _validator = vallidator;
        _logger = logger;
    }

    public async ValueTask<Result<List<ReadProductDto>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var result = Result.Failure<List<ReadProductDto>>(new Error("GetProducts.Validation", validationResult.ToString()));
            _logger.LogWarning("{Class}: {Method} - Warning: {ErrorCode} - {ErrorMessage}.", nameof(GetProductsHandler), nameof(Handle), result.Error.Code, result.Error.Message);
            return result;
        }
        var products = await _dbContext.Products.AsNoTracking().OrderByDescending(p => p.AddedDate).Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize).ToListAsync(cancellationToken);
        var productsDto = products.Select(product => new ReadProductDto(product.ProductId, product.Name, product.Description, product.Price, product.StockQuantity, product.AddedDate, product.LastModified)).ToList();
        _logger.LogInformation("{Class}: {Method} - Success, retrieved: {ProductCount} orders.", nameof(GetProductsHandler), nameof(Handle), productsDto.Count);
        return Result.Success(productsDto);
    }
}

public class GetProductsQueryValidator : AbstractValidator<GetProductsQuery>
{
    public GetProductsQueryValidator()
    {
        RuleFor(p => p.PageNumber).GreaterThan(0).WithMessage("Page Number must be greater than 0");
    }
}