using D2Store.Api.Features.Orders.Domain;
using D2Store.Api.Features.Orders.Dto;
using D2Store.Api.Features.Products.Domain;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using FluentValidation;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Orders;

public record CreateOrderCommand(Guid CustomerId, List<WriteOrderProductDtoCreate> Products) : IRequest<Result<ReadOrderDto>>;

public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, Result<ReadOrderDto>>
{
    private readonly AppDbContext _dbContext;
    private readonly IValidator<CreateOrderCommand> _validator;

    public CreateOrderHandler(AppDbContext dbContext, IValidator<CreateOrderCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async ValueTask<Result<ReadOrderDto>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var requestValidationResult = await ValidateRequestAsync(request, cancellationToken);
        if (requestValidationResult.IsFailure)
        {
            return Result.Failure<ReadOrderDto>(requestValidationResult.Error);
        }
        var customerExistsResult = await CustomerExistsAsync(request.CustomerId, cancellationToken);
        if (customerExistsResult.IsFailure)
        {
            return Result.Failure<ReadOrderDto>(customerExistsResult.Error);
        }
        var productsDict = await GetProductsDictionaryAsync(request.Products.Select(p => p.ProductId).Distinct().ToList(), cancellationToken);
        var validateProductsExistanceResult = ValidateProductsExistance(request, productsDict);
        if (validateProductsExistanceResult.IsFailure)
        {
            return Result.Failure<ReadOrderDto>(validateProductsExistanceResult.Error);
        }
        var stockCheckResult = ValidateStockAvailability(request, productsDict);
        if (stockCheckResult.IsFailure)
        {
            return Result.Failure<ReadOrderDto>(stockCheckResult.Error);
        }
        var createOrderResult = await CreateOrderAsync(request, productsDict, cancellationToken);
        var productDtos = MapOrderProductsToDto(createOrderResult.Value.Products.ToList());
        var orderDto = MapToReadOrderDto(createOrderResult.Value, productDtos);
        return Result.Success(orderDto);
    }

    private async Task<Result> ValidateRequestAsync(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure(new Error("CreateOrder.Validation", validationResult.ToString()));
        }
        return Result.Success();
    }

    private async Task<Result> CustomerExistsAsync(Guid customerId, CancellationToken cancellationToken)
    {
        var customerExists = await _dbContext.Customers.AsNoTracking().AnyAsync(c => c.CustomerId == customerId, cancellationToken);
        if (!customerExists)
        {
            return Result.Failure(new Error("CreateOrder.Validation", "Customer does not exist."));
        }
        return Result.Success();
    }

    private async Task<Dictionary<Guid, Product>> GetProductsDictionaryAsync(List<Guid> productIds, CancellationToken cancellationToken)
    {
        return await _dbContext.Products
            .Where(p => productIds
            .Contains(p.ProductId))
            .ToDictionaryAsync(p => p.ProductId, cancellationToken);
    }

    private static Result ValidateProductsExistance(CreateOrderCommand request, Dictionary<Guid, Product> productsDict)
    {
        foreach (var product in request.Products)
        {
            if (!productsDict.ContainsKey(product.ProductId))
            {
                return Result.Failure(new Error(
                    "CreateOrder.Validation",
                    $"Product with ID '{product.ProductId}' does not exist."));
            }
        }
        return Result.Success();
    }

    private static Result ValidateStockAvailability(CreateOrderCommand request, Dictionary<Guid, Product> productsDict)
    {
        foreach (var orderProduct in request.Products)
        {
            var product = productsDict[orderProduct.ProductId];
            var stockCheck = product.HasSufficientStock(orderProduct.Quantity);
            if (stockCheck.IsFailure)
            {
                return stockCheck;
            }
        }
        return Result.Success();
    }

    private async Task<Result<Order>> CreateOrderAsync(CreateOrderCommand request, Dictionary<Guid, Product> productsDict, CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            decimal totalAmount = CalculateTotalAmount(request, productsDict);
            var orderResult = Order.Create(request.CustomerId, totalAmount);
            _dbContext.Orders.Add(orderResult.Value);
            foreach (var orderProd in request.Products)
            {
                var product = productsDict[orderProd.ProductId];
                orderResult.Value.AddProduct(product, orderProd.Quantity);
                product.ReduceStock(orderProd.Quantity);
            }
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Result.Success(orderResult.Value);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static decimal CalculateTotalAmount(CreateOrderCommand request, Dictionary<Guid, Product> productsDict)
    {
        decimal total = 0;
        foreach (var orderProduct in request.Products)
        {
            var product = productsDict[orderProduct.ProductId];
            total += product.Price * orderProduct.Quantity;
        }
        return total;
    }

    private List<ReadOrderProductDto> MapOrderProductsToDto(List<OrderProduct> orderProducts)
    {
        return orderProducts.Select(op => new ReadOrderProductDto(
            op.Product.ProductId,
            op.Product.Name,
            op.Product.Description,
            op.Product.Price,
            op.Quantity
        )).ToList();
    }

    private static ReadOrderDto MapToReadOrderDto(Order order, List<ReadOrderProductDto> products)
    {
        return new ReadOrderDto(
            order.OrderId,
            order.CustomerId,
            products,
            order.OrderDate,
            order.TotalAmount,
            order.Status,
            order.LastModified);
    }
}

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(c => c.CustomerId).NotEmpty().WithMessage("Customer Id is required.");
        RuleFor(c => c.Products).NotEmpty().WithMessage("At least one product must be provided.");
        RuleForEach(c => c.Products).ChildRules(products =>
        {
            products.RuleFor(p => p.ProductId).NotEmpty().WithMessage("Product Id is required.");
            products.RuleFor(p => p.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than zero.");
        });
    }
}
