using D2Store.Api.Features.Customers.Domain;
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

    /// <summary>
    /// Coordinates retrieval and mapping and deletion of a specific customer.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<Result<Guid>> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await GetCustomerAsync(request.CustomerId, cancellationToken);
        if (customer == null)
        {
            return CreateCustomerNotFoundResult();
        }
        var orderExists = await _dbContext.Orders.AsNoTracking().AnyAsync(o => o.CustomerId == request.CustomerId, cancellationToken);
        if (orderExists)
        {
            return CreateOrdersExistForThisCustomerResult();
        }
        return await DeleteCustomerAsync(customer, cancellationToken);
    }

    /// <summary>
    /// Loads a customer object based on the CustomerId.
    /// </summary>
    /// <param name="customerId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Customer?> GetCustomerAsync(Guid customerId, CancellationToken cancellationToken)
    {
        return await _dbContext.Customers
            .FirstOrDefaultAsync(c => c.CustomerId == customerId, cancellationToken);
    }

    /// <summary>
    /// Creates a failure result response for when a specified customer cannot be found.
    /// </summary>
    /// <returns></returns>
    private static Result<Guid> CreateCustomerNotFoundResult()
    {
        return Result.Failure<Guid>(new Error(
            "GetCustomerById.Validation",
            "The customer with the specified Customer Id was not found."));
    }

    /// <summary>
    /// Creates a failure result response for when a customer that is to be deleted has orders against it. 
    /// </summary>
    /// <returns></returns>
    private static Result<Guid> CreateOrdersExistForThisCustomerResult()
    {
        return Result.Failure<Guid>(new Error(
            "DeleteCustomer.Validation",
            "Orders exist for this customer."));
    }

    /// <summary>
    /// Deletes the specified customer and persists the changes to the database table. 
    /// </summary>
    /// <param name="product"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Result<Guid>> DeleteCustomerAsync(Customer customer, CancellationToken cancellationToken)
    {
        _dbContext.Customers.Remove(customer);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(customer.CustomerId);
    }
}
