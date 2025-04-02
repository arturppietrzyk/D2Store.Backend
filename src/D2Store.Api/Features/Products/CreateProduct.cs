using D2Store.Api.Features.Orders;
using D2Store.Api.Features.Products.Domain;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using FluentValidation;
using MediatR;

namespace D2Store.Api.Features.Products;

public record CreateProductCommand(string Name, string Description, decimal Price, int StockQuantity) : IRequest<Result<Guid>>;

public class CreateProductHandler : IRequestHandler<CreateProductCommand, Result<Guid>>
{
    private readonly AppDbContext _dbContext;
    private readonly IValidator<CreateProductCommand> _validator;
    private readonly ILogger<CreateProductHandler> _logger;

    public CreateProductHandler(AppDbContext dbContext, IValidator<CreateProductCommand> validator, ILogger<CreateProductHandler> logger)
    {
        _dbContext = dbContext;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var inputValidationResult = Result.Failure<Guid>(new Error("CreateProduct.Validation", validationResult.ToString()));
            _logger.LogWarning("{Class}: {Method} - Warning: {ErrorCode} - {ErrorMessage}.", nameof(CreateProductHandler), nameof(Handle), inputValidationResult.Error.Code, inputValidationResult.Error.Message);
            return inputValidationResult;
        }
        var product = new Product(request.Name, request.Description, request.Price, request.StockQuantity);
        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("{Class}: {Method} - Success, created: {productId}.", nameof(CreateOrderHandler), nameof(Handle), product.ProductId.ToString());
        return Result.Success(product.ProductId);
    }
}

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(p => p.Name).NotEmpty().WithMessage("Name is required");
        RuleFor(p => p.Description).NotEmpty().WithMessage("Description is required");
        RuleFor(p => p.Price).GreaterThan(0).WithMessage("Price must be greater than zero");
        RuleFor(p => p.StockQuantity).GreaterThan(0).WithMessage("Stock Quantity must be greater than zero");
    }
}