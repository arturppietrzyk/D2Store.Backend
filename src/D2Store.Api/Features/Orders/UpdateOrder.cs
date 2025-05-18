//using D2Store.Api.Features.Orders.Domain;
//using D2Store.Api.Features.Orders.Dto;
//using D2Store.Api.Infrastructure;
//using D2Store.Api.Shared;
//using FluentValidation;
//using Mediator;
//using Microsoft.EntityFrameworkCore;

//namespace D2Store.Api.Features.Orders;

//public record UpdateOrderCommand(Guid OrderId, string? Status) : IRequest<Result<ReadOrderDto>>;

//public class UpdateOrderHandler : IRequestHandler<UpdateOrderCommand, Result<ReadOrderDto>>
//{
//    private readonly AppDbContext _dbContext;
//    private readonly IValidator<UpdateOrderCommand> _validator;

//    public UpdateOrderHandler(AppDbContext dbContext, IValidator<UpdateOrderCommand> validator)
//    {
//        _dbContext = dbContext;
//        _validator = validator;
//    }

//    /// <summary>
//    /// Coordinates validation, retrieval, mapping and updating of a specific order. Returns the updated order and its products into a response DTO.
//    /// </summary>
//    /// <param name="request"></param>
//    /// <param name="cancellationToken"></param>
//    /// <returns></returns>
//    public async ValueTask<Result<ReadOrderDto>> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
//    {
//        var validationResult = await ValidateRequestAsync(request, cancellationToken);
//        if (validationResult.IsFailure)
//        {
//            return Result.Failure<ReadOrderDto>(validationResult.Error);
//        }
//        var order = await GetOrderAsync(request.OrderId, cancellationToken);
//        if (order is null)
//        {
//            return CreateOrderNotFoundResult();
//        }
//        await UpdateOrderAsync(order, request, cancellationToken);
//        var productDtos = MapOrderProductsToDto(order.Products);
//        return Result.Success(MapToReadOrderDto(order, productDtos));
//    }

//    /// <summary>
//    /// Validates the input. 
//    /// </summary>
//    /// <param name="request"></param>
//    /// <param name="cancellationToken"></param>
//    /// <returns></returns>
//    private async Task<Result> ValidateRequestAsync(UpdateOrderCommand request, CancellationToken cancellationToken)
//    {
//        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
//        if (!validationResult.IsValid)
//        {
//            return Result.Failure<ReadOrderDto>(new Error("UpdateOrder.Validation", validationResult.ToString()));
//        }
//        return Result.Success();
//    }

//    /// <summary>
//    /// Loads an order object based on the OrderId, and eagerly loads its associated order products along with the product details for each item.
//    /// </summary>
//    /// <param name="orderId"></param>
//    /// <param name="cancellationToken"></param>
//    /// <returns></returns>
//    private async Task<Order?> GetOrderAsync(Guid orderId, CancellationToken cancellationToken)
//    {
//        return await _dbContext.Orders
//            .Include(o => o.Products)
//            .ThenInclude(op => op.Product)
//            .FirstOrDefaultAsync(o => o.OrderId == orderId, cancellationToken);
//    }

//    /// <summary>
//    /// Creates a failure result response for when a specified order cannot be found. 
//    /// </summary>
//    /// <returns></returns>
//    private static Result<ReadOrderDto> CreateOrderNotFoundResult()
//    {
//        return Result.Failure<ReadOrderDto>(new Error(
//            "UpdateOrder.Validation",
//            "The order with the specified Order Id was not found."));
//    }

//    /// <summary>
//    /// Updates the order and persists the changes in the database table. 
//    /// </summary>
//    /// <param name="order"></param>
//    /// <param name="request"></param>
//    /// <param name="cancellationToken"></param>
//    /// <returns></returns>
//    private async Task<Result<Order>> UpdateOrderAsync(Order order, UpdateOrderCommand request, CancellationToken cancellationToken)
//    {
//        order.UpdateOrderInfo(request.Status);
//        await _dbContext.SaveChangesAsync(cancellationToken);
//        return Result.Success(order);
//    }

//    /// <summary>
//    /// Maps the list of order products into the equivalent ReadOrderProductDto list. 
//    /// </summary>
//    /// <param name="orderProducts"></param>
//    /// <returns></returns>
//    private List<ReadOrderProductDto> MapOrderProductsToDto(List<OrderProduct> orderProducts)
//    {
//        return orderProducts.Select(op => new ReadOrderProductDto(
//            op.Product.ProductId,
//            op.Product.Name,
//            op.Product.Description,
//            op.Product.Price,
//            op.Quantity
//        )).ToList();
//    }

//    /// <summary>
//    /// Maps the retrieved order list of ReadOrderProductDto into a response object that gets returned when the GetOrderById endpoint is called. 
//    /// </summary>
//    /// <param name="order"></param>
//    /// <param name="products"></param>
//    /// <returns></returns>
//    private static ReadOrderDto MapToReadOrderDto(Order order, List<ReadOrderProductDto> products)
//    {
//        return new ReadOrderDto(
//            order.OrderId,
//            order.CustomerId,
//            products,
//            order.OrderDate,
//            order.TotalAmount,
//            order.Status,
//            order.LastModified);
//    }
//}

//public class UpdateOrderCommandValidator : AbstractValidator<UpdateOrderCommand>
//{
//    public UpdateOrderCommandValidator()
//    {
//        RuleFor(o => o.Status).NotEmpty().When(o => o.Status is not null).WithMessage("Status cannot be empty if provided.");
//    }
//}