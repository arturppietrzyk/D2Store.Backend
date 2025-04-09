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

    public GetOrdersHandler(AppDbContext dbContext, IValidator<GetOrdersQuery> vallidator)
    {
        _dbContext = dbContext;
        _validator = vallidator;
    }

    public async ValueTask<Result<List<ReadOrderDto>>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var result = Result.Failure<List<ReadOrderDto>>(new Error("GetOrders.Validation", validationResult.ToString()));
            return result;
        }
        var orders = await _dbContext.Orders.AsNoTracking().OrderByDescending(o => o.OrderDate).Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize).ToListAsync(cancellationToken);
        var ordersDto = orders.Select(order => new ReadOrderDto(order.OrderId, order.CustomerId, order.OrderDate, order.TotalAmount, order.Status, order.LastModified)).ToList();
        return Result.Success(ordersDto);
    }
}

public class GetOrdersQueryValidator : AbstractValidator<GetOrdersQuery>
{
    public GetOrdersQueryValidator()
    {
        RuleFor(p => p.PageNumber).GreaterThan(0).WithMessage("Page Number must be greater than 0");
    }
}