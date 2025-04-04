using D2Store.Api.Features.Customers;
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
    private readonly ILogger<UpdateProductHandler> _logger;

    public UpdateProductHandler(AppDbContext dbContext, IValidator<UpdateProductCommand> validator, ILogger<UpdateProductHandler> logger)
    {
        _dbContext = dbContext;
        _validator = validator;
        _logger = logger;
    }

    public async ValueTask<Result<ReadProductDto>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var inputValidationResult = Result.Failure<ReadProductDto>(new Error("UpdateProduct.Validation", validationResult.ToString()));
            _logger.LogWarning("{Class}: {Method} - Warning: {ErrorCode} - {ErrorMessage}.", nameof(UpdateProductHandler), nameof(Handle), inputValidationResult.Error.Code, inputValidationResult.Error.Message);
            return inputValidationResult;
        }
        var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.ProductId == request.ProductId, cancellationToken);
        if (product is null)
        {
            var result = Result.Failure<ReadProductDto>(new Error("UpdateProduct.Validation", "Product not found."));
            _logger.LogWarning("{Class}: {Method} - Warning: {ErrorCode} - {ErrorMessage}.", nameof(UpdateProductHandler), nameof(Handle), result.Error.Code, result.Error.Message);
            return result;
        }
        product.UpdateProductInfo(request.Name, request.Description, request.Price, request.StockQuantity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        var updatedProduct = new ReadProductDto(product.ProductId, product.Name, product.Description, product.Price, product.StockQuantity, product.AddedDate, product.LastModified);
        _logger.LogInformation("{Class}: {Method} - Success, updated: {productId}.", nameof(UpdateCustomerHandler), nameof(Handle), updatedProduct.ProductId);
        return Result.Success(updatedProduct);
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