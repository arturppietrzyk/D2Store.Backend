using D2Store.Api.Features.Orders.Dto;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Orders;

public record GetOrderByIdQuery(Guid OrderId) : IRequest<Result<ReadOrderDto>>;

public class GetOrderByIdHandler : IRequestHandler<GetOrderByIdQuery, Result<ReadOrderDto>>
{
    private readonly AppDbContext _dbContext;

    public GetOrderByIdHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask<Result<ReadOrderDto>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.OrderId == request.OrderId, cancellationToken);
        if (order is null)
        {
            var result = Result.Failure<ReadOrderDto>(new Error("GetOrderById.Validation", "The order with the specified Order Id was not found."));
            return result;
        }
        var orderdDto = new ReadOrderDto(order.OrderId, order.CustomerId, order.OrderDate, order.TotalAmount, order.Status, order.LastModified);
        return Result.Success<ReadOrderDto>(orderdDto);
    }
}