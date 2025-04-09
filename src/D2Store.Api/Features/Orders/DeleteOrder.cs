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
        var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.OrderId == request.OrderId, cancellationToken);
        if (order is null)
        {
            var result = Result.Failure<Guid>(new Error("DeleteOrder.Validation", "Order not found."));
            return result;
        }
        _dbContext.Orders.Remove(order);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(order.OrderId);
    }
}