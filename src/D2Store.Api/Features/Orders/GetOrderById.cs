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
        var productDtos = MapOrderProductsToDto(order.Products);
        return Result.Success(MapToReadOrderDto(order, productDtos));
    }

    private async Task<Order?> GetOrderAsync(Guid orderId, CancellationToken cancellationToken)
    {
        return await _dbContext.Orders
            .AsNoTracking()
            .Include(o => o.Products)
                .ThenInclude(op => op.Product)
            .FirstOrDefaultAsync(o => o.OrderId == orderId, cancellationToken);
    }

    private static Result<ReadOrderDto> CreateOrderNotFoundResult()
    {
        return Result.Failure<ReadOrderDto>(new Error(
            "GetOrderById.Validation",
            "The order with the specified Order Id was not found."));
    }

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
