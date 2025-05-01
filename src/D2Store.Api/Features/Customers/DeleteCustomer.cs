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
    /// Coordinates retrieval, validation and deletion of a specific customer. Returns the Guid of the deleted customer if successful. 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<Result<Guid>> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await GetCustomerAsync(request.CustomerId, cancellationToken);
        if (customer == null)
        {
            return CustomerNotFoundResult();
        }
        var ordersExist = await _dbContext.Orders.AsNoTracking().AnyAsync(o => o.CustomerId == request.CustomerId, cancellationToken);
        var deleteCustomerResult = await DeleteCustomerAsync(customer, ordersExist, cancellationToken);
        return deleteCustomerResult;
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
    private static Result<Guid> CustomerNotFoundResult()
    {
        return Result.Failure<Guid>(new Error(
            "DeleteCustomer.Validation",
            "The customer with the specified Customer Id was not found."));
    }

    /// <summary>
    /// Validates the business rules and deletes the specified customer, persisting the changes to the database table. 
    /// </summary>
    /// <param name="product"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Result<Guid>> DeleteCustomerAsync(Customer customer, bool ordersExist, CancellationToken cancellationToken)
    {
        var deleteCustomerResult = customer.Delete(ordersExist);
        if (deleteCustomerResult.IsFailure)
        {
            return Result.Failure<Guid>(deleteCustomerResult.Error);
        }
        _dbContext.Customers.Remove(customer);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(customer.CustomerId);
    }
}
