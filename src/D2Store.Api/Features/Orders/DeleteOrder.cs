using D2Store.Api.Features.Orders.Dto;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using MediatR;
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

    public async Task<Result<Guid>> Handle(DeleteOrderCommand request, CancellationToken cancellationToken) 
    {
        var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);
        if (order is null)
        {
            return Result.Failure<Guid>(new Error("DeleteOrder.NotFound", "Order not found."));
        }
        _dbContext.Orders.Remove(order);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(order.Id);
    }
}

