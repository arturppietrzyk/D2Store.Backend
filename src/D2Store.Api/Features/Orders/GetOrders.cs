using D2Store.Api.Features.Orders.Dto;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Orders;

public record GetOrderQuery() : IRequest <Result<List<ReadOrderDto>>>;

public class GetOrderHandler : IRequestHandler<GetOrderQuery, Result<List<ReadOrderDto>>>
{
    private readonly AppDbContext _dbContext;

    public GetOrderHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<List<ReadOrderDto>>> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        var orders = await _dbContext.Orders.AsNoTracking().ToListAsync(cancellationToken);
        if (orders is null || !orders.Any())
        {
            return Result.Failure<List<ReadOrderDto>>(new Error("GetOrders.NotFound", "No orders found."));
        }
        var orderDto = orders.Select(order => new ReadOrderDto(order.Id, order.CustomerId, order.OrderDate, order.TotalAmount, order.Status.ToString())).ToList();
        return Result.Success(orderDto);
    }
}
