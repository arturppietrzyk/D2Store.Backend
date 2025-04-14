using D2Store.Api.Features.Orders.Domain;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Orders;

public record DeleteOrderCommand(Guid OrderId) : IRequest<Result<Guid>>; 

public class DeleteOrderHander : IRequestHandler<DeleteOrderCommand, Result<Guid>> 
{
    private readonly AppDbContext _dbContext;

    public DeleteOrderHander(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask<Result<Guid>> Handle(DeleteOrderCommand request, CancellationToken cancellationToken) 
    {
        var order = await GetOrderAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return CreateOrderNotFoundResult();
        }
        return await DeleteOrderAsync(order, cancellationToken);
    }

    /// <summary>
    /// Find the specific order based on OrderId.
    /// </summary>
    /// <param name="orderId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Order?> GetOrderAsync(Guid orderId, CancellationToken cancellationToken)
    {
        return await _dbContext.Orders.Include(o => o.Products).FirstOrDefaultAsync(o => o.OrderId == orderId, cancellationToken);
    }

    /// <summary>
    /// Create a failure result for when a specific order could not be found in the orders table. 
    /// </summary>
    /// <returns></returns>
    private static Result<Guid> CreateOrderNotFoundResult()
    {
        return Result.Failure<Guid>(new Error(
            "DeleteOrder.Validation",
            "The order with the specified Order Id was not found."));
    }

    /// <summary>
    /// Wraps the deletion of OrderProducts of a specific order as well as the order itself in a transaction so everything can be rolled back if an error occurs.
    /// </summary>
    /// <param name="order"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Result<Guid>> DeleteOrderAsync(Order order, CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            if (order.Products != null && order.Products.Any())
            {
                _dbContext.OrderProducts.RemoveRange(order.Products);
                await _dbContext.SaveChangesAsync();
            }
            _dbContext.Orders.Remove(order);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Result.Success(order.OrderId);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}