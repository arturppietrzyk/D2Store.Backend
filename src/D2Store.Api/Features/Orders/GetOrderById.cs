using D2Store.Api.Features.Orders.Domain;
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
        var order = await GetOrderAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return CreateOrderNotFoundResult();
        }
        var orderProducts = await GetOrderProductsAsync(order.OrderId, cancellationToken);
        return Result.Success(MapToReadOrderDto(order, orderProducts));
    }

    /// <summary>
    /// Find the specific order based on OrderId.
    /// </summary>
    /// <param name="orderId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Order?> GetOrderAsync(Guid orderId, CancellationToken cancellationToken)
    {
        return await _dbContext.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.OrderId == orderId, cancellationToken);
    }

    /// <summary>
    /// Get all the OrderProducts by a specified OrderId, a join it up to the Products table using the ProductIds to create a list of order products for a given order. 
    /// </summary>
    /// <param name="orderId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<List<ReadOrderProductDto>> GetOrderProductsAsync(Guid orderId, CancellationToken cancellationToken)
    {
        return await _dbContext.OrderProducts
            .AsNoTracking()
            .Where(op => op.OrderId == orderId)
            .Join(
                _dbContext.Products,
                op => op.ProductId,
                p => p.ProductId,
                (op, p) => new ReadOrderProductDto(
                    p.ProductId,
                    p.Name,
                    p.Description,
                    p.Price,
                    op.Quantity
                )
            )
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Create a failure result for when a specific order could not be found in the orders table. 
    /// </summary>
    /// <returns></returns>
    private static Result<ReadOrderDto> CreateOrderNotFoundResult()
    {
        return Result.Failure<ReadOrderDto>(new Error(
            "GetOrderById.Validation",
            "The order with the specified Order Id was not found."));
    }

    /// <summary>
    /// Create the Response dto object that the GetOrderById endpoint comes back with after it is queried. 
    /// </summary>
    /// <param name="order"></param>
    /// <param name="products"></param>
    /// <returns></returns>
    private static ReadOrderDto MapToReadOrderDto(Order order, List<ReadOrderProductDto> products)
    {
        return new ReadOrderDto(
            order.OrderId,
            order.CustomerId,
            products,
            order.OrderDate,
            order.TotalAmount,
            order.Status,
            order.LastModified);
    }
}