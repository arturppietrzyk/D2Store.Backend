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

    public GetProductsHandler(AppDbContext dbContext, IValidator<GetProductsQuery> vallidator)
    {
        _dbContext = dbContext;
        _validator = vallidator;
    }

    public async ValueTask<Result<List<ReadProductDto>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var result = Result.Failure<List<ReadProductDto>>(new Error("GetProducts.Validation", validationResult.ToString()));
            return result;
        }
        var products = await _dbContext.Products.AsNoTracking().OrderByDescending(p => p.AddedDate).Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize).ToListAsync(cancellationToken);
        var productsDto = products.Select(product => new ReadProductDto(product.ProductId, product.Name, product.Description, product.Price, product.StockQuantity, product.AddedDate, product.LastModified)).ToList();
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