using D2Store.Api.Features.Orders.Domain;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Orders;

public record DeleteOrderCommand(Guid OrderId, Guid AuthenticatedUserId, bool IsAdmin) : IRequest<Result<Guid>>;

public class DeleteOrderHander : IRequestHandler<DeleteOrderCommand, Result<Guid>>
{
    private readonly AppDbContext _dbContext;

    public DeleteOrderHander(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Coordinates retrieval and mapping and deletion of a specific order and its order products.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<Result<Guid>> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
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
        var deleteOrder = await DeleteOrderAsync(orderResult.Value, cancellationToken);
        return Result.Success(deleteOrder);
    }

    /// <summary>
    /// Loads an order object based on the OrderId, and eagerly loads its associated order products along with the product details for each item.
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
        if(order is null)
        {
            return Result.Failure<Order>(new Error(
            "DeleteOrder.Validation",
            "The order with the specified Order Id was not found."));
        }
        return Result.Success(order);
    }

    /// <summary>
    /// Wraps the deletion of order products of a specific order as well as the order itself in a transaction so everything can be rolled back if an error occurs.
    /// </summary>
    /// <param name="order"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Guid> DeleteOrderAsync(Order order, CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            if (order.Products is not null && order.Products.Any())
            {
                _dbContext.OrderProducts.RemoveRange(order.Products);
                await _dbContext.SaveChangesAsync();
            }
            _dbContext.Orders.Remove(order);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return order.OrderId;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}