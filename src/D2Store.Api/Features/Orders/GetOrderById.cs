using D2Store.Api.Features.Orders.Dto;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Orders;

public record GetOrderByIdQuery(Guid Id) : IRequest<Result<ReadOrderDto?>>;

public class GetOrderByIdHandler : IRequestHandler<GetOrderByIdQuery, Result<ReadOrderDto?>>
{
    private readonly AppDbContext _dbContext;

    public GetOrderByIdHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<ReadOrderDto?>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);
        if (order is null)
        {
            return Result.Failure<ReadOrderDto?>(new Error("GetOrderById.Null", "The order with the specified Id was not found"));
        }
        var dto = new ReadOrderDto(order.Id, order.CustomerId, order.OrderDate, order.TotalAmount, order.Status.ToString());
        return Result.Success<ReadOrderDto?>(dto);
    }
}