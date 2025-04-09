using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Customers;

public record DeleteCustomerCommand(Guid CustomerId) : IRequest<Result<Guid>>;

public class DeleteCustomerHandler : IRequestHandler<DeleteCustomerCommand, Result<Guid>>
{
    private readonly AppDbContext _dbContext;

    public DeleteCustomerHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask<Result<Guid>> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _dbContext.Customers.FirstOrDefaultAsync(c => c.CustomerId == request.CustomerId, cancellationToken);
        if (customer is null)
        {
            var result = Result.Failure<Guid>(new Error("DeleteCustomer.Validation", "Customer not found."));
            return result;
        }
        var orderExists = await _dbContext.Orders.AnyAsync(o => o.CustomerId == request.CustomerId, cancellationToken);
        if (orderExists)
        {
            var result = Result.Failure<Guid>(new Error("DeleteCustomer.Validation", "Orders exist for this customer."));
            return result;
        }
        _dbContext.Customers.Remove(customer);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(customer.CustomerId);
    }
}
