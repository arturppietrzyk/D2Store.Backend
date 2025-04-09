using D2Store.Api.Features.Products.Domain;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using FluentValidation;
using Mediator;

namespace D2Store.Api.Features.Products;

public record CreateProductCommand(string Name, string Description, decimal Price, int StockQuantity) : IRequest<Result<Guid>>;

public class CreateProductHandler : IRequestHandler<CreateProductCommand, Result<Guid>>
{
    private readonly AppDbContext _dbContext;
    private readonly IValidator<CreateProductCommand> _validator;

    public CreateProductHandler(AppDbContext dbContext, IValidator<CreateProductCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async ValueTask<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var inputValidationResult = Result.Failure<Guid>(new Error("CreateProduct.Validation", validationResult.ToString()));
            return inputValidationResult;
        }
        var product = new Product(request.Name, request.Description, request.Price, request.StockQuantity);
        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync(cancellationToken);
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