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
        var order = await _dbContext.Orders.Include(o => o.Products).FirstOrDefaultAsync(o => o.OrderId == request.OrderId, cancellationToken);
        if (order is null)
        {
            var result = Result.Failure<Guid>(new Error("DeleteOrder.Validation", "Order not found."));
            return result;
        }
        return await DeleteOrderAsync(order, cancellationToken);
    }

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