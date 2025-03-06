using D2Store.Api.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Orders;

public record DeleteOrderCommand(Guid OrderId) : IRequest<Guid>; 

public class DeleteOrderHander : IRequestHandler<DeleteOrderCommand, Guid> 
{
    private readonly AppDbContext _dbContext;

    public DeleteOrderHander(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(DeleteOrderCommand request, CancellationToken cancellationToken) 
    {
        var order = await _dbContext.Orders
           .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);
        //if (order == null)
        //{
        //    throw new KeyNotFoundException($"Order with ID {request.OrderId} not found.");
        //}
        _dbContext.Orders.Remove(order);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return order.Id;
    }
}

