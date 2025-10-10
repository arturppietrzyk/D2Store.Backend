using D2Store.Api.Features.Orders.Domain;
using D2Store.Api.Features.Orders.Dto;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using FluentValidation;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Orders;

public record GetOrdersForUserQuery(Guid UserId, int PageNumber, int PageSize, Guid AuthenticatedUserId, bool IsAdmin) : IRequest<Result<IReadOnlyCollection<ReadOrderDto>>>;

public class GetOrdersForUserHandler : IRequestHandler<GetOrdersForUserQuery, Result<IReadOnlyCollection<ReadOrderDto>>>
{
    private readonly AppDbContext _dbContext;
    private readonly IValidator<GetOrdersForUserQuery> _validator;

    public GetOrdersForUserHandler(AppDbContext dbContext, IValidator<GetOrdersForUserQuery> validator)
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
    public async ValueTask<Result<IReadOnlyCollection<ReadOrderDto>>> Handle(GetOrdersForUserQuery request, CancellationToken cancellationToken)
    {
        if (!request.IsAdmin && request.UserId != request.AuthenticatedUserId)
        {
            return Result.Failure<IReadOnlyCollection<ReadOrderDto>>(Error.Forbidden);
        }
        var validationResult = await ValidateRequestAsync(request, cancellationToken);
        if (validationResult.IsFailure)
        {
            return Result.Failure<IReadOnlyCollection<ReadOrderDto>>(validationResult.Error);
        }
        var orders = await GetPaginatedOrdersWithProductsAsync(request.UserId, request.PageNumber, request.PageSize, cancellationToken);
        var ordersDto = orders.Select(MapToReadOrderDto).ToList();
        return Result.Success<IReadOnlyCollection<ReadOrderDto>>(ordersDto);
    }

    /// <summary>
    /// Loads the order objects based on the Pagination parameters and UserId. It then eagerly loads its associated order products along with the product details for each item.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<List<Order>> GetPaginatedOrdersWithProductsAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        return await _dbContext.Orders
            .AsNoTracking()
            .Where(o => o.UserId == userId)
            .Include(o => o.Products)
            .ThenInclude(op => op.Product)
            .OrderByDescending(o => o.OrderDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Validates the PageNumber and PageSize Pagination parameters. 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Result> ValidateRequestAsync(GetOrdersForUserQuery request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure(new Error("GetOrdersForUser.Validation", validationResult.ToString()));
        }
        return Result.Success();
    }

    /// <summary>
    /// Maps an Order entity and its associated products to a ReadOrderDto. 
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
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
            order.Status.ToString(),
            order.LastModified);
    }
}

public class GetOrdersForUserQueryValidator : AbstractValidator<GetOrdersForUserQuery>
{
    public GetOrdersForUserQueryValidator()
    {
        RuleFor(p => p.PageNumber).GreaterThan(0).WithMessage("Page Number must be greater than 0.");
        RuleFor(p => p.PageSize).GreaterThan(0).WithMessage("Page Size must be greater than 0.");
    }
}