using D2Store.Api.Features.Customers.Dto;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Customers;

public record UpdateCustomerCommand(Guid CustomerId, string? FirstName, string? LastName, string? Email, string? PhoneNumber, string? Address) : IRequest<Result<ReadCustomerDto>>;

public class UpdateCustomerHandler : IRequestHandler<UpdateCustomerCommand, Result<ReadCustomerDto>>
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<UpdateCustomerHandler> _logger;

    public UpdateCustomerHandler(AppDbContext dbContext, ILogger<UpdateCustomerHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<ReadCustomerDto>> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _dbContext.Customers.FirstOrDefaultAsync(c => c.CustomerId == request.CustomerId, cancellationToken);
        if (customer is null)
        {
            var result = Result.Failure<ReadCustomerDto>(new Error("UpdateCustomer.NotFound", "Customer not found."));
            _logger.LogWarning("{Class}: {Method} - Warning: {ErrorCode} - {ErrorMessage}.", nameof(UpdateCustomerHandler), nameof(Handle), result.Error.Code, result.Error.Message);
            return result;
        }
        customer.UpdateCustomerInfo(request.FirstName, request.LastName, request.Email, request.PhoneNumber, request.Address);
        await _dbContext.SaveChangesAsync(cancellationToken);
        var updatedCustomer = new ReadCustomerDto(customer.CustomerId, customer.FirstName, customer.LastName, customer.Email, customer.PhoneNumber, customer.Address, customer.CreatedAt, customer.LastModified
        );
        _logger.LogInformation("{Class}: {Method} - Success, updated: {customerId}.", nameof(UpdateCustomerHandler), nameof(Handle), updatedCustomer.CustomerId);
        return Result.Success(updatedCustomer);
    }
}