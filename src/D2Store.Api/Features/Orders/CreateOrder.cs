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
        var validationResult = await ValidateRequestAsync(request, cancellationToken);
        if (validationResult.IsFailure)
        {
            return Result.Failure<ReadOrderDto>(validationResult.Error);
        }
        var customerExists = await CustomerExistsAsync(request.CustomerId, cancellationToken);
        if (!customerExists)
        {
            return Result.Failure<ReadOrderDto>(new Error("CreateOrder.Validation", "Customer does not exist."));
        }
        var productsDict = await GetProductsDictionaryAsync(request.Products.Select(p => p.ProductId).Distinct().ToList(), cancellationToken);
        var totalAmountResult = CalculateTotalAmount(request.Products, productsDict);
        if (totalAmountResult.IsFailure)
        {
            return Result.Failure<ReadOrderDto>(totalAmountResult.Error);
        }
        decimal totalAmount = totalAmountResult.Value;
        var createOrder = await CreateOrderAndOrderProductsAsync(request, productsDict, totalAmount, cancellationToken);
        var order = await GetOrderAsync(createOrder.Value.OrderId, cancellationToken);
        if (order is null)
        {
            return CreateOrderNotFoundResult();
        }
        var productDtos = MapOrderProductsToDto(order.Products);
        return Result.Success(MapToReadOrderDto(order, productDtos));
    }

    /// <summary>
    /// Executes the input validation done by the Fluent Validation class CreateOrderCommandValidator. 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Result> ValidateRequestAsync(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure<Order>(new Error("CreateOrder.Validation", validationResult.ToString()));
        }
        return Result.Success();
    }

    /// <summary>
    /// Does a check in the customers table to see if the customer making the order even exists. This is validated up the call stack and a validation error is thrown if this is false.
    /// </summary>
    /// <param name="customerId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<bool> CustomerExistsAsync(Guid customerId, CancellationToken cancellationToken)
    {
        return await _dbContext.Customers.AsNoTracking().AnyAsync(c => c.CustomerId == customerId, cancellationToken);
    }

    /// <summary>
    /// Runs the ValidateProductAvailablity method, if successful the TotalAmount is calcuated, else a validation error is thrown. 
    /// </summary>
    /// <param name="products"></param>
    /// <param name="productsDict"></param>
    /// <returns></returns>
    private Result<decimal> CalculateTotalAmount(List<WriteOrderProductDtoCreate> orderProducts, Dictionary<Guid, Product> productsDict)
    {
        decimal totalAmount = 0;
        foreach (var orderProduct in orderProducts)
        {
            var validationResult = ValidateProductAvailability(orderProduct, productsDict);
            if (validationResult.IsFailure)
            {
                return Result.Failure<decimal>(validationResult.Error);
            }
            totalAmount += validationResult.Value.Price * orderProduct.Quantity;
        }
        return Result.Success(totalAmount);
    }

    /// <summary>
    /// Validates the Product for its existance and available quantity. 
    /// </summary>
    /// <param name="productOrder"></param>
    /// <param name="productsDict"></param>
    /// <returns></returns>
    private Result<Product> ValidateProductAvailability(WriteOrderProductDtoCreate orderProduct, Dictionary<Guid, Product> productsDict)
    {
        if (!productsDict.TryGetValue(orderProduct.ProductId, out var product))
        {
            return Result.Failure<Product>(new Error("CreateOrder.Validation", $"Product {orderProduct.ProductId} does not exist."));
        }
        if (product.StockQuantity < orderProduct.Quantity)
        {
            return Result.Failure<Product>(new Error("CreateOrder.Validation", $"Not enough stock of product {product.ProductId} to fulfill the order."));
        }
        return Result.Success(product);
    }

    /// <summary>
    /// Creates the Order object, presists the Order in the Orders table. 
    /// Loops through each product from the request and updates the quantities of the Products from the order inside the Products table. 
    /// Adds the necessary entries to the OrderProducts junction table. 
    /// This whole code is wrapped in a transaction so if any of the steps fail, all the changes get rolled back. 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="productsDict"></param>
    /// <param name="totalAmount"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Result<Order>> CreateOrderAndOrderProductsAsync(CreateOrderCommand request, Dictionary<Guid, Product> productsDict, decimal totalAmount, CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var order = new Order(request.CustomerId, totalAmount);
            _dbContext.Orders.Add(order);
            await _dbContext.SaveChangesAsync(cancellationToken);
            foreach (var orderProd in request.Products)
            {
                var product = productsDict[orderProd.ProductId];
                product.UpdateProductInfo(null, null, null, product.StockQuantity - orderProd.Quantity);
                var orderProduct = new OrderProduct(order.OrderId, product.ProductId, orderProd.Quantity);
                _dbContext.OrderProducts.Add(orderProduct);
            }
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Result.Success(order);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Takes all the productIds from the input and returns a dictionary of all the products based on those id's. This is done so only one trip to the Products table needs to be made and any subsequent methods who need this data for the order process can have constant time look ups into the dictionary for this information. 
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
    /// Create a failure result for when a specific order could not be found in the orders table. 
    /// </summary>
    /// <returns></returns>
    private static Result<ReadOrderDto> CreateOrderNotFoundResult()
    {
        return Result.Failure<ReadOrderDto>(new Error(
            "GetOrderById.Validation",
            "The order with the specified Order Id was not found."));
    }

    ///
    /// <summary>
    /// Find the specific order based on OrderId.
    /// </summary>
    /// <param name="orderId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Order?> GetOrderAsync(Guid orderId, CancellationToken cancellationToken)
    {
        return await _dbContext.Orders
           .AsNoTracking()
           .Include(o => o.Products)
               .ThenInclude(op => op.Product)
           .FirstOrDefaultAsync(o => o.OrderId == orderId, cancellationToken);
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

/// <summary>
/// Fluent Validation which validates input. 
/// </summary>
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
