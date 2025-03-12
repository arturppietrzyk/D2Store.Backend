using D2Store.Api.Features.Orders.Domain;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using MediatR;

namespace D2Store.Api.Features.Orders;

public record CreateOrderCommand(Guid CustomerId, decimal TotalAmount) : IRequest<Result<Guid>>;

public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, Result<Guid>>
{
    private readonly AppDbContext _dbContext;

    public CreateOrderHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {

        // Example validation: Check if customer exists (optional)
        //var customerExists = await _dbContext.Customers
        //    .AnyAsync(c => c.Id == request.CustomerId, cancellationToken);
        //if (!customerExists)
        //{
        //    return Result.Failure<Guid>(new Error("CreateOrder.CustomerNotFound", "Customer not found."));
        //}

        var order = new Order(request.CustomerId, request.TotalAmount);
        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(order.Id);
    }
}
