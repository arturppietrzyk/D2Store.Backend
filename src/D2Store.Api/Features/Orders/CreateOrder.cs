using D2Store.Api.Features.Orders.Domain;
using D2Store.Api.Features.Orders.Dto;
using D2Store.Api.Features.Products.Domain;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using FluentValidation;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Orders;

public record CreateOrderCommand(Guid UserId, List<WriteOrderProductDtoCreate> Products, Guid AuthenticatedUserId, bool IsAdmin) : IRequest<Result<ReadOrderDto>>;

public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, Result<ReadOrderDto>>
{
    private readonly AppDbContext _dbContext;
    private readonly IValidator<CreateOrderCommand> _validator;

    public CreateOrderHandler(AppDbContext dbContext, IValidator<CreateOrderCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    /// <summary>
    /// Coordinates validation, retrieval, mapping and creating of an order. Returns the created order and its products into a response DTO.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<Result<ReadOrderDto>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        if (!request.IsAdmin && request.UserId != request.AuthenticatedUserId)
        {
            return Result.Failure<ReadOrderDto>(Error.Forbidden);
        }
        var requestValidationResult = await ValidateRequestAsync(request, cancellationToken);
        if (requestValidationResult.IsFailure)
        {
            return Result.Failure<ReadOrderDto>(requestValidationResult.Error);
        }
        var userExists = await _dbContext.Users.AsNoTracking().AnyAsync(u => u.UserId == request.UserId, cancellationToken);
        var assertCustomerExistanceResult = Order.AssertCustomerExsistance(userExists);
        if (assertCustomerExistanceResult.IsFailure)
        {
            return Result.Failure<ReadOrderDto>(assertCustomerExistanceResult.Error);
        }
        var productsDict = await GetProductsDictionaryAsync(request.Products.Select(p => p.ProductId).Distinct().ToList(), cancellationToken);
        var assertProductsExistanceResult = Order.AssertProductsExistance(request, productsDict);
        if (assertProductsExistanceResult.IsFailure)
        {
            return Result.Failure<ReadOrderDto>(assertProductsExistanceResult.Error);
        }
        var assertStockAvailabilityResult = Order.AssertStockAvailability(request, productsDict);
        if (assertStockAvailabilityResult.IsFailure)
        {
            return Result.Failure<ReadOrderDto>(assertStockAvailabilityResult.Error);
        }
        var createOrder = await CreateOrderAsync(request, productsDict, cancellationToken);
        var productDtos = MapOrderProductsToDto(createOrder.Products.ToList());
        var orderDto = MapToReadOrderDto(createOrder, productDtos);
        return Result.Success(orderDto);
    }

    /// <summary>
    /// Validates the input.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Result> ValidateRequestAsync(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure(new Error("CreateOrder.Validation", validationResult.ToString()));
        }
        return Result.Success();
    }

    /// <summary>
    /// Loads all the products from the order into a dictionary of products for constant time product lookups. 
    /// </summary>
    /// <param name="productIds"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Dictionary<Guid, Product>> GetProductsDictionaryAsync(List<Guid> productIds, CancellationToken cancellationToken)
    {
        return await _dbContext.Products
            .Where(p => productIds
            .Contains(p.ProductId))
            .ToDictionaryAsync(p => p.ProductId, cancellationToken);
    }

    /// <summary>
    /// Creates the order inside the Orders table and adds an entry for each product from the order inside the OrderProducts table. 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="productsDict"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Order> CreateOrderAsync(CreateOrderCommand request, Dictionary<Guid, Product> productsDict, CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            decimal totalAmount = Order.CalculateTotalAmount(request, productsDict);
            var order = Order.Create(request.UserId, totalAmount);
            _dbContext.Orders.Add(order);
            foreach (var orderProd in request.Products)
            {
                var product = productsDict[orderProd.ProductId];
                order.AddProduct(product, orderProd.Quantity);
                product.ReduceStock(orderProd.Quantity);
            }
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return order;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Maps the list of order products into the equivalent ReadOrderProductDto list. 
    /// </summary>
    /// <param name="orderProducts"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Maps the retrieved order list of ReadOrderProductDto into a response object that gets returned when the GetOrderById endpoint is called. 
    /// </summary>
    /// <param name="order"></param>
    /// <param name="products"></param>
    /// <returns></returns>
    private static ReadOrderDto MapToReadOrderDto(Order order, List<ReadOrderProductDto> products)
    {
        return new ReadOrderDto(
            order.OrderId,
            order.UserId,
            products,
            order.OrderDate,
            order.TotalAmount,
            order.Status.ToString(),
            order.LastModified);
    }
}

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(u => u.UserId).NotEmpty().WithMessage("User Id is required.");
        RuleFor(c => c.Products).NotEmpty().WithMessage("At least one product must be provided.");
        RuleForEach(c => c.Products).ChildRules(products =>
        {
            products.RuleFor(p => p.ProductId).NotEmpty().WithMessage("Product Id is required.");
            products.RuleFor(p => p.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than zero.");
        });
    }
}
