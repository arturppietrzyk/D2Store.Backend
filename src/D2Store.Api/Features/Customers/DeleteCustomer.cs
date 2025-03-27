using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Customers;

public record DeleteCustomerCommand(Guid CustomerId) : IRequest<Result<Guid>>;

public class DeleteCustomerHandler : IRequestHandler<DeleteCustomerCommand, Result<Guid>>
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<DeleteCustomerHandler> _logger;

    public DeleteCustomerHandler(AppDbContext dbContext, ILogger<DeleteCustomerHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _dbContext.Customers.FirstOrDefaultAsync(c => c.CustomerId == request.CustomerId, cancellationToken);
        if (customer is null)
        {
            var result = Result.Failure<Guid>(new Error("DeleteCustomer.NotFound", "Customer not found."));
            _logger.LogWarning("{Class}: {Method} - Warning: {ErrorCode} - {ErrorMessage}.", nameof(DeleteCustomerHandler), nameof(Handle), result.Error.Code, result.Error.Message);
            return result;
        }
        var orderExists = await _dbContext.Orders.AnyAsync(o => o.CustomerId == request.CustomerId, cancellationToken);
        if (orderExists)
        {
            var result = Result.Failure<Guid>(new Error("DeleteCustomer.Validation", "Orders exist for this customer."));
            _logger.LogWarning("{Class}: {Method} - Warning: {ErrorCode} - {ErrorMessage}.", nameof(DeleteCustomerHandler), nameof(Handle), result.Error.Code, result.Error.Message);
            return result;
        }
        _dbContext.Customers.Remove(customer);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("{Class}: {Method} - Success, deleted {customerId}.", nameof(DeleteCustomerHandler), nameof(Handle), customer.CustomerId);
        return Result.Success(customer.CustomerId);
    }
}
