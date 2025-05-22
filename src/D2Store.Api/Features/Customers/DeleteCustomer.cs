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
        var customerResult = await GetCustomerAsync(request.CustomerId, cancellationToken);
        if (customerResult.IsFailure)
        {
            return Result.Failure<Guid>(customerResult.Error);
        }
        var hasOrders = await _dbContext.Orders.AsNoTracking().AnyAsync(o => o.CustomerId == request.CustomerId, cancellationToken);
        var validateOrdersExsistanceResult = Customer.ValidateOrdersExistance(hasOrders);
        if (validateOrdersExsistanceResult.IsFailure)
        {
            return Result.Failure<Guid>(validateOrdersExsistanceResult.Error);
        }
        var deleteCustomer = await DeleteCustomerAsync(customerResult.Value, cancellationToken);
        return Result.Success(deleteCustomer);
    }

    /// <summary>
    /// Loads a customer object based on the CustomerId.
    /// </summary>
    /// <param name="customerId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Result<Customer>> GetCustomerAsync(Guid customerId, CancellationToken cancellationToken)
    {
        var customer = await _dbContext.Customers
            .FirstOrDefaultAsync(c => c.CustomerId == customerId, cancellationToken);
        if(customer is null)
        {
            return Result.Failure<Customer>(new Error(
           "DeleteCustomer.Validation",
           "The customer with the specified Customer Id was not found."));
        }
        return Result.Success(customer);
    }

    /// <summary>
    /// Deletes the specified customer, persisting the changes to the database table. 
    /// </summary>
    /// <param name="product"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Guid> DeleteCustomerAsync(Customer customer, CancellationToken cancellationToken)
    {
        _dbContext.Customers.Remove(customer);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return customer.CustomerId;
    }
}
