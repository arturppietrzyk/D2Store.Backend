using D2Store.Api.Features.Customers.Domain;
using D2Store.Api.Features.Customers.Dto;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Customers;

public record GetCustomerByIdQuery(Guid CustomerId) : IRequest<Result<ReadCustomerDto>>;

public class GetCustomerByIdHandler : IRequestHandler<GetCustomerByIdQuery, Result<ReadCustomerDto>>
{
    private readonly AppDbContext _dbContext;

    public GetCustomerByIdHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Coordinates retrieval and mapping of a specific customer into a response DTO.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<Result<ReadCustomerDto>> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await GetCustomerAsync(request.CustomerId, cancellationToken);
        if(customer is null)
        {
            return CreateCustomerNotFoundResult();
        }
        return Result.Success(MapToReadCustomerDto(customer));
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
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CustomerId == customerId, cancellationToken);
    }

    /// <summary>
    /// Creates a failure result response for when a specified customer cannot be found.
    /// </summary>
    /// <returns></returns>
    private static Result<ReadCustomerDto> CreateCustomerNotFoundResult()
    {
        return Result.Failure<ReadCustomerDto>(new Error(
            "GetCustomerById.Validation",
            "The customer with the specified Customer Id was not found."));
    }

    /// <summary>
    /// Maps the retrieved customer into the ReadCustomerDto which is returned as the response. 
    /// </summary>
    /// <param name="customer"></param>
    /// <returns></returns>
    private static ReadCustomerDto MapToReadCustomerDto(Customer customer)
    {
        return new ReadCustomerDto(
            customer.CustomerId,
            customer.FirstName,
            customer.LastName,
            customer.Email,
            customer.PhoneNumber,
            customer.Address,
            customer.CreatedDate,
            customer.LastModified);
    }
}
