using D2Store.Api.Features.Orders.Dto;
using D2Store.Api.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Orders;

public record UpdateOrderCommand(Guid Id, decimal? TotalAmount, string? Status) : IRequest<ReadOrderDto>;

public class UpdateOrderHandler : IRequestHandler<UpdateOrderCommand, ReadOrderDto>
{
    private readonly AppDbContext _dbContext;

    public UpdateOrderHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ReadOrderDto> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);
        if (request.TotalAmount.HasValue)
        {
            order.TotalAmount = request.TotalAmount.Value;
        }
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            order.Status = request.Status;
        }
        await _dbContext.SaveChangesAsync(cancellationToken);
        return new ReadOrderDto(order.Id, order.CustomerId, order.OrderDate, order.TotalAmount, order.Status);
    }
}
