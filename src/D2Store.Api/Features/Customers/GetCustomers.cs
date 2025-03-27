using D2Store.Api.Features.Customers.Dto;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Customers;

public record GetCustomersQuery() : IRequest<Result<List<ReadCustomerDto>>>;

public class GetCustomersHandler : IRequestHandler<GetCustomersQuery, Result<List<ReadCustomerDto>>>
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<GetCustomersHandler> _logger;

    public GetCustomersHandler(AppDbContext dbContext, ILogger<GetCustomersHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<List<ReadCustomerDto>>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
    {
        var customers = await _dbContext.Customers.AsNoTracking().ToListAsync(cancellationToken);
        if (customers is null || !customers.Any())
        {
            var result = Result.Failure<List<ReadCustomerDto>>(new Error("GetCustomers.NotFound", "No customers found."));
            _logger.LogWarning("{Class}: {Method} - Warning: {ErrorCode} - {ErrorMessage}.", nameof(GetCustomersHandler), nameof(Handle), result.Error.Code, result.Error.Message);
            return result;
        }
        var customersDto = customers.Select(customer => new ReadCustomerDto(customer.CustomerId, customer.FirstName, customer.LastName, customer.Email, customer.PhoneNumber, customer.Address, customer.CreatedAt)).ToList();
        _logger.LogInformation("{Class}: {Method} - Success, retrieved: {CustomerCount} customers.", nameof(GetCustomersHandler), nameof(Handle), customersDto.Count);
        return Result.Success(customersDto);
    }
}
