using D2Store.Api.Features.Orders.Domain;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Orders;

public record GetOrderByIdQuery(Guid id) : IRequest<Result<Order?>>;

public class GetOrderByIdHandler : IRequestHandler<GetOrderByIdQuery, Result<Order?>>
{
    private readonly AppDbContext _dbContext;

    public GetOrderByIdHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Order?>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.Id == request.id, cancellationToken);
        if(order is null) 
        {
            return Result.Failure<Order?>(new Error("GetOrderById.Null","The order with the specified Id was not found"));
        }
        return Result.Success<Order?>(order);
    }
}
