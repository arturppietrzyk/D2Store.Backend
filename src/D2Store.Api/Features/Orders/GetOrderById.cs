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

    /// <summary>
    /// Coordinates retrieval and mapping of a specific order and its products into a response DTO.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<Result<ReadOrderDto>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await GetOrderAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return CreateOrderNotFoundResult();
        }
        var orderProductDtos = MapOrderProductsToDto(order.Products);
        return Result.Success(MapToReadOrderDto(order, orderProductDtos));
    }

    /// <summary>
    /// Loads an order object based on the OrderId, and eagerly loads its associated order products along with the product details for each item.
    /// </summary>
    /// <param name="orderId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Order?> GetOrderAsync(Guid orderId, CancellationToken cancellationToken)
    {
        return await _dbContext.Orders
            .AsNoTracking()
            .Include(o => o.Products)
            .ThenInclude(op => op.Product)
            .FirstOrDefaultAsync(o => o.OrderId == orderId, cancellationToken);
    }

    /// <summary>
    /// Creates a failure result response for when a specified order cannot be found. 
    /// </summary>
    /// <returns></returns>
    private static Result<ReadOrderDto> CreateOrderNotFoundResult()
    {
        return Result.Failure<ReadOrderDto>(new Error(
            "GetOrderById.Validation",
            "The order with the specified Order Id was not found."));
    }

    /// <summary>
    /// Maps the list of order products into the equivalent ReadOrderProductDto list. 
    /// </summary>
    /// <param name="orderProducts"></param>
    /// <returns></returns>
    private List<ReadOrderProductDto> MapOrderProductsToDto(List<OrderProduct> orderProducts)
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
            order.CustomerId,
            products,
            order.OrderDate,
            order.TotalAmount,
            order.Status,
            order.LastModified);
    }
}
