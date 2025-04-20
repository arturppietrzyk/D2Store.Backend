using D2Store.Api.Features.Customers.Dto;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using FluentValidation;
using Mediator;
using Microsoft.EntityFrameworkCore;
using D2Store.Api.Features.Customers.Domain;

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

    /// <summary>
    /// Coordinates validation, retrieval and mapping of the specific customers into response DTO.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<Result<List<ReadCustomerDto>>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
    {
        var validationResult = await ValidateRequestAsync(request, cancellationToken);
        if (validationResult.IsFailure)
        {
            return Result.Failure<List<ReadCustomerDto>>(validationResult.Error);
        }
        var customers = await GetPaginatedCustomersAsync(request.PageNumber, request.PageSize, cancellationToken);
        var customerDtos = customers.Select(MapToReadProductDto).ToList();
        return Result.Success(customerDtos);
    }

    /// <summary>
    /// Validates the PageNumber and PageSize Pagination parameters. 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Result> ValidateRequestAsync(GetCustomersQuery request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure<List<ReadCustomerDto>>(new Error("GetCustomers.Validation", validationResult.ToString()));
        }
        return Result.Success();
    }

    /// <summary>
    /// Loads the customer objects based on the Pagination parameters.
    /// </summary>
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<List<Customer>> GetPaginatedCustomersAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        return await _dbContext.Customers
            .AsNoTracking()
            .OrderByDescending(c => c.CreatedDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Maps an customer entity into a ReadCustomerDto. 
    /// </summary>
    /// <param name="customer"></param>
    /// <returns></returns>
    private static ReadCustomerDto MapToReadProductDto(Customer customer)
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

public class GetCustomersQueryValidator : AbstractValidator<GetCustomersQuery>
{
    public GetCustomersQueryValidator()
    {
        RuleFor(p => p.PageNumber).GreaterThan(0).WithMessage("Page Number must be greater than 0");
        RuleFor(p => p.PageSize).GreaterThan(0).WithMessage("Page Size must be greater than 0");
    }
}