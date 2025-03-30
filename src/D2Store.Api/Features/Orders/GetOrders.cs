using D2Store.Api.Features.Orders.Dto;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Orders;

public record GetOrdersQuery() : IRequest <Result<List<ReadOrderDto>>>;

public class GetOrdersHandler : IRequestHandler<GetOrdersQuery, Result<List<ReadOrderDto>>>
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<GetOrdersHandler> _logger;

    public GetOrdersHandler(AppDbContext dbContext, ILogger<GetOrdersHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<List<ReadOrderDto>>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await _dbContext.Orders.AsNoTracking().ToListAsync(cancellationToken);
        var ordersDto = orders.Select(order => new ReadOrderDto(order.OrderId, order.CustomerId, order.OrderDate, order.TotalAmount, order.Status.ToString())).ToList();
        _logger.LogInformation("{Class}: {Method} - Success, retrieved: {OrderCount} orders.",nameof(GetOrdersHandler), nameof(Handle), ordersDto.Count);
        return Result.Success(ordersDto);
    }
}
