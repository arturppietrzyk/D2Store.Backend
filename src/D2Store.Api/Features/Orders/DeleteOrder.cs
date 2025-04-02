using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Orders;

public record DeleteOrderCommand(Guid OrderId) : IRequest<Result<Guid>>; 

public class DeleteOrderHander : IRequestHandler<DeleteOrderCommand, Result<Guid>> 
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<DeleteOrderHander> _logger;

    public DeleteOrderHander(AppDbContext dbContext, ILogger<DeleteOrderHander> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(DeleteOrderCommand request, CancellationToken cancellationToken) 
    {
        var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.OrderId == request.OrderId, cancellationToken);
        if (order is null)
        {
            var result = Result.Failure<Guid>(new Error("DeleteOrder.Validation", "Order not found."));
            _logger.LogWarning("{Class}: {Method} - Warning: {ErrorCode} - {ErrorMessage}.", nameof(DeleteOrderHander), nameof(Handle), result.Error.Code, result.Error.Message);
            return result;
        }
        _dbContext.Orders.Remove(order);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("{Class}: {Method} - Success, deleted {orderId}.", nameof(GetOrdersHandler), nameof(Handle), order.OrderId);
        return Result.Success(order.OrderId);
    }
}