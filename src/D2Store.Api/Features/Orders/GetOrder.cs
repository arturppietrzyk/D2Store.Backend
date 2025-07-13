using D2Store.Api.Features.Orders.Domain;
using D2Store.Api.Features.Orders.Dto;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Orders;

public record GetOrderQuery(Guid OrderId, Guid AuthenticatedUserId, bool IsAdmin) : IRequest<Result<ReadOrderDto>>;

public class GetOrderHandler : IRequestHandler<GetOrderQuery, Result<ReadOrderDto>>
{
    private readonly AppDbContext _dbContext;

    public GetOrderHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Coordinates retrieval and mapping of a specific order and its products into a response DTO.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<Result<ReadOrderDto>> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        var orderResult = await GetOrderAsync(request.OrderId, cancellationToken);
        if (orderResult.IsFailure)
        {
            return Result.Failure<ReadOrderDto>(orderResult.Error);
        }
        var order = orderResult.Value;
        if (!request.IsAdmin && order.UserId != request.AuthenticatedUserId)
        {
            return Result.Failure<ReadOrderDto>(Error.Forbidden);
        }
        var orderProductDtos = MapOrderProductsToDto(orderResult.Value.Products);
        return Result.Success(MapToReadOrderDto(orderResult.Value, orderProductDtos));
    }

    /// <summary>
    /// Loads an order object based on the OrderId, and eagerly loads its associated order products along with the product details for each item.
    /// </summary>
    /// <param name="orderId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Result<Order>> GetOrderAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var orderExists = await _dbContext.Orders
         .Include(o => o.Products)
         .ThenInclude(op => op.Product)
         .FirstOrDefaultAsync(o => o.OrderId == orderId, cancellationToken);
        if (orderExists is null)
        {
            return Result.Failure<Order>(new Error(
            "GetOrderById.Validation",
            "The order with the specified Order Id was not found."));
        }
        return Result.Success(orderExists);
    }

    /// <summary>
    /// Maps the list of order products into the equivalent ReadOrderProductDto list. 
    /// </summary>
    /// <param name="orderProducts"></param>
    /// <returns></returns>
    private List<ReadOrderProductDto> MapOrderProductsToDto(IReadOnlyCollection<OrderProduct> orderProducts)
    {
        return orderProducts.Select(op => new ReadOrderProductDto(
            op.Product.ProductId,
            op.Product.Name,
            op.Product.Description,
            op.Product.Price,
            op.Quantity
        )).ToList();
    }

    /// <summary>
    /// Maps the retrieved order into a response object that gets returned when the GetOrderById endpoint is called. 
    /// </summary>
    /// <param name="order"></param>
    /// <param name="products"></param>
    /// <returns></returns>
    private static ReadOrderDto MapToReadOrderDto(Order order, List<ReadOrderProductDto> products)
    {
        return new ReadOrderDto(
            order.OrderId,
            order.UserId,
            products,
            order.OrderDate,
            order.TotalAmount,
            order.Status.ToString(),
            order.LastModified);
    }
}
