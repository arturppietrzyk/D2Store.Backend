using D2Store.Api.Features.Orders.Domain;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using D2Store.Api.Shared.Enums;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Orders;

public record UpdateOrderCommand(Guid OrderId, OrderStatus Status, Guid AuthenticatedUserId, bool IsAdmin) : IRequest<Result>;

public class UpdateOrderHandler : IRequestHandler<UpdateOrderCommand, Result>
{
    private readonly AppDbContext _dbContext;

    public UpdateOrderHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Coordinates retrieval and updating of a specific order.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<Result> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        var orderResult = await GetOrderAsync(request.OrderId, cancellationToken);
        if (orderResult.IsFailure)
        {
            return Result.Failure(orderResult.Error);
        }
        var order = orderResult.Value;
        if (!request.IsAdmin && order.UserId != request.AuthenticatedUserId)
        {
            return Result.Failure(Error.Forbidden);
        }
        var updateOrder = await UpdateOrderAsync(orderResult.Value, request, cancellationToken);
        if (updateOrder.IsFailure)
        {
            return Result.Failure(updateOrder.Error);
        }
        return Result.Success();
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
        var order = await _dbContext.Orders
            .Include(o => o.Products)
            .ThenInclude(op => op.Product)
            .FirstOrDefaultAsync(o => o.OrderId == orderId, cancellationToken);
        if (order is null)
        {
            return Result.Failure<Order>(Error.NotFound);
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
    private async Task<Result> UpdateOrderAsync(Order order, UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        var isUpdatedResult = order.Update(request.Status);
        if (isUpdatedResult.IsFailure)
        {
            return Result.Failure(isUpdatedResult.Error);
        }
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(order.OrderId);
    }
}