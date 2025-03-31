using D2Store.Api.Features.Customers.Dto;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Customers;

public record GetCustomersQuery(int PageNumber, int PageSize) : IRequest<Result<List<ReadCustomerDto>>>;

public class GetCustomersHandler : IRequestHandler<GetCustomersQuery, Result<List<ReadCustomerDto>>>
{
    private readonly AppDbContext _dbContext;
    private readonly IValidator<GetCustomersQuery> _validator;
    private readonly ILogger<GetCustomersHandler> _logger;

    public GetCustomersHandler(AppDbContext dbContext, IValidator<GetCustomersQuery> validator, ILogger<GetCustomersHandler> logger)
    {
        _dbContext = dbContext;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<List<ReadCustomerDto>>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var result = Result.Failure<List<ReadCustomerDto>>(new Error("GetCustomers.Validation", validationResult.ToString()));
            _logger.LogWarning("{Class}: {Method} - Warning: {ErrorCode} - {ErrorMessage}.", nameof(GetCustomersHandler), nameof(Handle), result.Error.Code, result.Error.Message);
            return result;
        }
        var customers = await _dbContext.Customers.AsNoTracking().OrderByDescending(c => c.CreatedAt).Skip((request.PageNumber -1) * request.PageSize).Take(request.PageSize).ToListAsync(cancellationToken);
        var customersDto = customers.Select(customer => new ReadCustomerDto(customer.CustomerId, customer.FirstName, customer.LastName, customer.Email, customer.PhoneNumber, customer.Address, customer.CreatedAt, customer.LastModified)).ToList();
        _logger.LogInformation("{Class}: {Method} - Success, retrieved: {CustomerCount} customers.", nameof(GetCustomersHandler), nameof(Handle), customersDto.Count);
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