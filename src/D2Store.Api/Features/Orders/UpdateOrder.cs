using D2Store.Api.Features.Orders.Domain;
using D2Store.Api.Features.Orders.Dto;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using FluentValidation;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Orders;

public record UpdateOrderCommand(Guid OrderId, string? Status) : IRequest<Result<ReadOrderDto>>;

public class UpdateOrderHandler : IRequestHandler<UpdateOrderCommand, Result<ReadOrderDto>>
{
    private readonly AppDbContext _dbContext;
    private readonly IValidator<UpdateOrderCommand> _validator;

    public UpdateOrderHandler(AppDbContext dbContext, IValidator<UpdateOrderCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async ValueTask<Result<ReadOrderDto>> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await ValidateRequestAsync(request, cancellationToken);
        if (validationResult.IsFailure)
        {
            return Result.Failure<ReadOrderDto>(validationResult.Error);
        }
        var order = await GetOrderAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return CreateOrderNotFoundResult();
        }
        order.UpdateOrderInfo(request.Status);
        await _dbContext.SaveChangesAsync(cancellationToken);
        var orderProducts = await GetOrderProductsAsync(order.OrderId, cancellationToken);
        return Result.Success(MapToReadOrderDto(order, orderProducts));
    }

    /// <summary>
    /// Executes the input validation done by the Fluent Validation class UpdateOrderCommandValidator.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Result> ValidateRequestAsync(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure<ReadOrderDto>(new Error("UpdateOrder.Validation", validationResult.ToString()));
        }
        return Result.Success();
    }

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
            .FirstOrDefaultAsync(o => o.OrderId == orderId, cancellationToken);
    }

    /// <summary>
    /// Get all the OrderProducts by a specified OrderId, a join it up to the Products table using the ProductIds to create a list of order products for a given order. 
    /// </summary>
    /// <param name="orderId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<List<ReadOrderProductDto>> GetOrderProductsAsync(Guid orderId, CancellationToken cancellationToken)
    {
        return await _dbContext.OrderProducts
            .AsNoTracking()
            .Where(op => op.OrderId == orderId)
            .Join(
                _dbContext.Products,
                op => op.ProductId,
                p => p.ProductId,
                (op, p) => new ReadOrderProductDto(
                    p.ProductId,
                    p.Name,
                    p.Description,
                    p.Price,
                    op.Quantity
                )
            )
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Create a failure result for when a specific order could not be found in the orders table. 
    /// </summary>
    /// <returns></returns>
    private static Result<ReadOrderDto> CreateOrderNotFoundResult()
    {
        return Result.Failure<ReadOrderDto>(new Error(
            "UpdateOrder.Validation",
            "The order with the specified Order Id was not found."));
    }

    /// <summary>
    /// Create the Response dto object that the GetOrderById endpoint comes back with after it is queried. 
    /// </summary>
    /// <param name="order"></param>
    /// <param name="products"></param>
    /// <returns></returns>
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

public class UpdateOrderCommandValidator : AbstractValidator<UpdateOrderCommand>
{
    public UpdateOrderCommandValidator()
    {
        RuleFor(o => o.Status).NotEmpty().When(o => o.Status is not null).WithMessage("Status cannot be empty if provided.");
    }
}