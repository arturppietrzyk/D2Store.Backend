using D2Store.Api.Features.Orders.Domain;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Orders;

public record DeleteOrderCommand(Guid OrderId, Guid AuthenticatedUserId, bool IsAdmin) : IRequest<Result>;

public class DeleteOrderHander : IRequestHandler<DeleteOrderCommand, Result>
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
    public async ValueTask<Result> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        var orderResult = await GetOrderAsync(request.OrderId, cancellationToken);
        if (orderResult.IsFailure)
        {
            return Result.Failure(orderResult.Error);
        }
        if (!request.IsAdmin && orderResult.Value.UserId != request.AuthenticatedUserId)
        {
            return Result.Failure(Error.Forbidden);
        }
        await DeleteOrderAsync(orderResult.Value, cancellationToken);
        return Result.Success();
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
        if (order is null)
        {
            return Result.Failure<Order>(Error.NotFound);
        }
        return Result.Success(order);
    }

    /// <summary>
    /// Deletes the specified order persisting the changes to the database table. 
    /// </summary>
    /// <param name="product"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task DeleteOrderAsync(Order order, CancellationToken cancellationToken)
    {
        _dbContext.Orders.Remove(order);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}