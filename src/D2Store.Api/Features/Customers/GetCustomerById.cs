using D2Store.Api.Features.Customers.Dto;
using D2Store.Api.Features.Orders;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Customers;

public record GetCustomerByIdQuery(Guid CustomerId) : IRequest<Result<ReadCustomerDto>>;

public class GetCustomerByIdHandler : IRequestHandler<GetCustomerByIdQuery, Result<ReadCustomerDto>>
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<GetCustomerByIdHandler> _logger;

    public GetCustomerByIdHandler(AppDbContext dbContext, ILogger<GetCustomerByIdHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<ReadCustomerDto>> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await _dbContext.Customers.AsNoTracking().FirstOrDefaultAsync(o => o.CustomerId == request.CustomerId, cancellationToken);
        if (customer is null)
        {
            var result = Result.Failure<ReadCustomerDto>(new Error("GetCustomerById.Validation", "The customer with the specified Customer Id was not found."));
            _logger.LogWarning("{Class}: {Method}- Warning: {ErrorCode} - {ErrorMessage}", nameof(GetCustomerByIdHandler), nameof(Handle), result.Error.Code, result.Error.Message);
            return result;
        }
        var customerDto = new ReadCustomerDto(customer.CustomerId, customer.FirstName, customer.LastName, customer.Email, customer.PhoneNumber, customer.Address, customer.CreatedAt, customer.LastModified);
        _logger.LogInformation("{Class}: {Method} - Success, retrieved: {orderId}.", nameof(GetOrdersHandler), nameof(Handle), customerDto.CustomerId.ToString());
        return Result.Success<ReadCustomerDto>(customerDto);
    }
}
