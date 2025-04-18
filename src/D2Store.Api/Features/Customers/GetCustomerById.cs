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

    public async ValueTask<Result<ReadCustomerDto>> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await _dbContext.Customers.AsNoTracking().FirstOrDefaultAsync(o => o.CustomerId == request.CustomerId, cancellationToken);
        if (customer is null)
        {
            var result = Result.Failure<ReadCustomerDto>(new Error("GetCustomerById.Validation", "The customer with the specified Customer Id was not found."));
            return result;
        }
        var customerDto = new ReadCustomerDto(customer.CustomerId, customer.FirstName, customer.LastName, customer.Email, customer.PhoneNumber, customer.Address, customer.CreatedDate, customer.LastModified);
        return Result.Success<ReadCustomerDto>(customerDto);
    }
}
