using D2Store.Api.Features.Orders.Dto;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Orders;

public record UpdateOrderCommand(Guid Id, decimal? TotalAmount, string? Status) : IRequest<Result<ReadOrderDto>>;

public class UpdateOrderHandler : IRequestHandler<UpdateOrderCommand, Result<ReadOrderDto>>
{
    private readonly AppDbContext _dbContext;

    public UpdateOrderHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<ReadOrderDto>> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);
        if (order is null)
        {
            return Result.Failure<ReadOrderDto>(new Error("UpdateOrder.NotFound", "Order not found."));
        }
        if (request.TotalAmount <= 0)
        {
            return Result.Failure<ReadOrderDto>(new Error("UpdateOrder.InvalidAmount", "Total amount must be greater than zero."));
        }
        order.UpdateTotalAmount(request.TotalAmount);
        order.UpdateStatus(request.Status);
        await _dbContext.SaveChangesAsync(cancellationToken);
        var updatedOrder = new ReadOrderDto(order.Id, order.CustomerId, order.OrderDate, order.TotalAmount, order.Status);
        return Result.Success(updatedOrder);
    }
}
