using D2Store.Api.Features.Customers.Dto;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using FluentValidation;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Customers;

public record GetCustomersQuery(int PageNumber, int PageSize) : IRequest<Result<List<ReadCustomerDto>>>;

public class GetCustomersHandler : IRequestHandler<GetCustomersQuery, Result<List<ReadCustomerDto>>>
{
    private readonly AppDbContext _dbContext;
    private readonly IValidator<GetCustomersQuery> _validator;

    public GetCustomersHandler(AppDbContext dbContext, IValidator<GetCustomersQuery> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async ValueTask<Result<List<ReadCustomerDto>>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var result = Result.Failure<List<ReadCustomerDto>>(new Error("GetCustomers.Validation", validationResult.ToString()));
            return result;
        }
        var customers = await _dbContext.Customers.AsNoTracking().OrderByDescending(c => c.CreatedDate).Skip((request.PageNumber -1) * request.PageSize).Take(request.PageSize).ToListAsync(cancellationToken);
        var customersDto = customers.Select(customer => new ReadCustomerDto(customer.CustomerId, customer.FirstName, customer.LastName, customer.Email, customer.PhoneNumber, customer.Address, customer.CreatedDate, customer.LastModified)).ToList();
        return Result.Success(customersDto);
    }
}

public class GetCustomersQueryValidator : AbstractValidator<GetCustomersQuery>
{
    public GetCustomersQueryValidator()
    {
        RuleFor(p => p.PageNumber).GreaterThan(0).WithMessage("Page Number must be greater than 0");
    }
}