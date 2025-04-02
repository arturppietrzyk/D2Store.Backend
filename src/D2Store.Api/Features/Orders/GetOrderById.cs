using D2Store.Api.Features.Orders.Dto;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Orders;

public record GetOrderByIdQuery(Guid OrderId) : IRequest<Result<ReadOrderDto?>>;

public class GetOrderByIdHandler : IRequestHandler<GetOrderByIdQuery, Result<ReadOrderDto?>>
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<GetOrderByIdHandler> _logger;

    public GetOrderByIdHandler(AppDbContext dbContext, ILogger<GetOrderByIdHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<ReadOrderDto?>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.OrderId == request.OrderId, cancellationToken);
        if (order is null)
        {
            var result = Result.Failure<ReadOrderDto?>(new Error("GetOrderById.Null", "The order with the specified Id was not found."));
            _logger.LogWarning("{Class}: {Method}- Warning: {ErrorCode} - {ErrorMessage}", nameof(GetOrderByIdHandler), nameof(Handle), result.Error.Code, result.Error.Message);
            return result;
        }
        var orderdDto = new ReadOrderDto(order.OrderId, order.CustomerId, order.OrderDate, order.TotalAmount, order.Status, order.LastModified);
        _logger.LogInformation("{Class}: {Method} - Success, retrieved: {orderId}.", nameof(GetOrdersHandler), nameof(Handle), orderdDto.OrderId.ToString());
        return Result.Success<ReadOrderDto?>(orderdDto);
    }
}