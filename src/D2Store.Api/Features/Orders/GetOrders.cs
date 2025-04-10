using D2Store.Api.Features.Orders.Domain;
using D2Store.Api.Features.Orders.Dto;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using FluentValidation;
using Mediator;
using Microsoft.EntityFrameworkCore;
namespace D2Store.Api.Features.Orders;

public record GetOrdersQuery(int PageNumber, int PageSize) : IRequest<Result<List<ReadOrderDto>>>;

public class GetOrdersHandler : IRequestHandler<GetOrdersQuery, Result<List<ReadOrderDto>>>
{
    private readonly AppDbContext _dbContext;
    private readonly IValidator<GetOrdersQuery> _validator;

    public GetOrdersHandler(AppDbContext dbContext, IValidator<GetOrdersQuery> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async ValueTask<Result<List<ReadOrderDto>>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var validationResult = await ValidateRequestAsync(request, cancellationToken);
        if (validationResult.IsFailure)
        {
            return Result.Failure<List<ReadOrderDto>>(validationResult.Error);
        }
        var orders = await GetPaginatedOrdersAsync(request.PageNumber, request.PageSize, cancellationToken);
        var orderIds = orders.Select(o => o.OrderId).ToList();
        var orderProducts = await GetOrderProductsForOrdersAsync(orderIds, cancellationToken);
        var orderDtos = orders.Select(order => MapToReadOrderDto(order, orderProducts
            .GetValueOrDefault(order.OrderId, new List<ReadOrderProductDto>())))
            .ToList();
        return Result.Success(orderDtos);
    }

    /// <summary>
    /// Executes the input validation relating to page number and page size, done by the Fluent Validation class CreateOrderCommandValidator. 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Result> ValidateRequestAsync(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure<List<ReadOrderDto>>(new Error("GetOrders.Validation", validationResult.ToString()));
        }
        return Result.Success();
    }

    /// <summary>
    /// Retrieves the specific orders based on the pagination parameters. 
    /// </summary>
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<List<Order>> GetPaginatedOrdersAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        return await _dbContext.Orders
            .AsNoTracking()
            .OrderByDescending(o => o.OrderDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Takes a list of OrderIds and grabs all the OrderProducts with those OrderIds, it then joins to the Products table using the ProductId. Once the joins are established, a ReadOrderProductDto is assembled a and a key is created which is the order guid while the value is the list of products for that order. 
    /// </summary>
    /// <param name="orderIds"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Dictionary<Guid, List<ReadOrderProductDto>>> GetOrderProductsForOrdersAsync(List<Guid> orderIds, CancellationToken cancellationToken)
    {
        return await _dbContext.OrderProducts
            .AsNoTracking()
            .Where(op => orderIds.Contains(op.OrderId))
            .Join(_dbContext.Products,
                op => op.ProductId,
                p => p.ProductId,
                (op, p) => new
                {
                    op.OrderId,
                    ProductDto = new ReadOrderProductDto(
                        p.ProductId,
                        p.Name,
                        p.Description,
                        p.Price,
                        op.Quantity)
                })
            .GroupBy(x => x.OrderId)
            .ToDictionaryAsync(g => g.Key, g => g
            .Select(x => x.ProductDto).ToList(), cancellationToken);
    }

    /// <summary>
    /// Maps the order fields to the ReadOrderDto. 
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

public class GetOrdersQueryValidator : AbstractValidator<GetOrdersQuery>
{
    public GetOrdersQueryValidator()
    {
        RuleFor(p => p.PageNumber).GreaterThan(0).WithMessage("Page Number must be greater than 0");
        RuleFor(p => p.PageSize).GreaterThan(0).WithMessage("Page Size must be greater than 0");
    }
}