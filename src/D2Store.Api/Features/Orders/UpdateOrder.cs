using D2Store.Api.Features.Orders.Domain;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using D2Store.Api.Shared.Enums;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Orders;

public record UpdateOrderCommand(Guid OrderId, OrderStatus? Status, Guid AuthenticatedUserId, bool IsAdmin) : IRequest<Result<Guid>>;

public class UpdateOrderHandler : IRequestHandler<UpdateOrderCommand, Result<Guid>>
{
    private readonly AppDbContext _dbContext;

    public UpdateOrderHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Coordinates validation, retrieval and updating of a specific order. Returns the Guid of the updated order if successful.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<Result<Guid>> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        var orderResult = await GetOrderAsync(request.OrderId, cancellationToken);
        if (orderResult.IsFailure)
        {
            return Result.Failure<Guid>(orderResult.Error);
        }
        var order = orderResult.Value;
        if (!request.IsAdmin && order.UserId != request.AuthenticatedUserId)
        {
            return Result.Failure<Guid>(Error.Forbidden);
        }
        var updateOrder = await UpdateOrderAsync(orderResult.Value, request, cancellationToken);
        return Result.Success(updateOrder);
    }

    /// <summary>
    /// Loads an order object based on the OrderId, and eagerly loads its associated order products along with the product details for each item.
    /// Validate whether an order got brought back and returns either a result success of result failure. 
    /// </summary>
    /// <param name="orderId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Result<Order>> GetOrderAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var order =  await _dbContext.Orders
            .Include(o => o.Products)
            .ThenInclude(op => op.Product)
            .FirstOrDefaultAsync(o => o.OrderId == orderId, cancellationToken);
        if (order is null)
        {
            return Result.Failure<Order>(new Error(
            "UpdateOrder.Validation",
            "The order with the specified Order Id was not found."));
        }
        return Result.Success(order);
    }

    /// <summary>
    /// Updates the order and persists the changes in the database table. 
    /// </summary>
    /// <param name="order"></param>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Guid> UpdateOrderAsync(Order order, UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        order.Update(request.Status);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return order.OrderId;
    }
}