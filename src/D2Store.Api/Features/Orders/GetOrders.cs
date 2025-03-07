using D2Store.Api.Features.Orders.Domain;
using D2Store.Api.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Orders;

public record GetOrderQuery() : IRequest<List<Order?>>;

public class GetOrderHandler : IRequestHandler<GetOrderQuery, List<Order?>>
{
    private readonly AppDbContext _dbContext;

    public GetOrderHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Order?>> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        var orders = await _dbContext.Orders.ToListAsync(cancellationToken);
        return orders;
    }
}
