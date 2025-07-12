using D2Store.Api.Features.Orders.Domain;
using D2Store.Api.Features.Orders.Dto;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using FluentValidation;
using Mediator;
using Microsoft.EntityFrameworkCore;
namespace D2Store.Api.Features.Orders;

public record GetOrdersQuery(int PageNumber, int PageSize, bool isAdmin) : IRequest<Result<List<ReadOrderDto>>>;

public class GetOrdersHandler : IRequestHandler<GetOrdersQuery, Result<List<ReadOrderDto>>>
{
    private readonly AppDbContext _dbContext;
    private readonly IValidator<GetOrdersQuery> _validator;

    public GetOrdersHandler(AppDbContext dbContext, IValidator<GetOrdersQuery> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    /// <summary>
    /// Coordinates validation, retrieval and mapping of the orders and its products into a response DTO.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<Result<List<ReadOrderDto>>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        if (!request.isAdmin)
        {
            return Result.Failure<List<ReadOrderDto>>(Error.Forbidden);
        }
        var validationResult = await ValidateRequestAsync(request, cancellationToken);
        if (validationResult.IsFailure)
        {
            return Result.Failure<List<ReadOrderDto>>(validationResult.Error);
        }
        var orders = await GetPaginatedOrdersWithProductsAsync(request.PageNumber, request.PageSize, cancellationToken);
        var orderDtos = orders.Select(MapToReadOrderDto).ToList();
        return Result.Success(orderDtos);
    }

    /// <summary>
    /// Validates the PageNumber and PageSize Pagination parameters. 
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
    /// Loads the order objects based on the Pagination parameters, and eagerly loads its associated order products along with the product details for each item.
    /// </summary>
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<List<Order>> GetPaginatedOrdersWithProductsAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        return await _dbContext.Orders
            .AsNoTracking()
            .Include(o => o.Products)
            .ThenInclude(op => op.Product)
            .OrderByDescending(o => o.OrderDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Maps an Order entity and its associated products to a ReadOrderDto. 
    /// </summary>
    private static ReadOrderDto MapToReadOrderDto(Order order)
    {
        var productDtos = order.Products.Select(op => new ReadOrderProductDto(
            op.Product.ProductId,
            op.Product.Name,
            op.Product.Description,
            op.Product.Price,
            op.Quantity)).ToList();
        return new ReadOrderDto(
            order.OrderId,
            order.UserId,
            productDtos,
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
        RuleFor(p => p.PageNumber).GreaterThan(0).WithMessage("Page Number must be greater than 0.");
        RuleFor(p => p.PageSize).GreaterThan(0).WithMessage("Page Size must be greater than 0.");
    }
}