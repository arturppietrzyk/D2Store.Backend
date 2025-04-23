using D2Store.Api.Features.Customers.Domain;
using D2Store.Api.Features.Customers.Dto;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using FluentValidation;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Customers;

public record UpdateCustomerCommand(Guid CustomerId, string? FirstName, string? LastName, string? Email, string? PhoneNumber, string? Address) : IRequest<Result<ReadCustomerDto>>;

public class UpdateCustomerHandler : IRequestHandler<UpdateCustomerCommand, Result<ReadCustomerDto>>
{
    private readonly AppDbContext _dbContext;
    private readonly IValidator<UpdateCustomerCommand> _validator;

    public UpdateCustomerHandler(AppDbContext dbContext, IValidator<UpdateCustomerCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    /// <summary>
    /// Coordinates validation, retrieval, mapping and updating of a specific customer. Returns the updated customer in a response DTO.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<Result<ReadCustomerDto>> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await ValidateRequestAsync(request, cancellationToken);
        if (validationResult.IsFailure)
        {
            return Result.Failure<ReadCustomerDto>(validationResult.Error);
        }
        var customer = await GetCustomerAsync(request.CustomerId, cancellationToken);
        if (customer == null)
        {
            return CreateCustomerNotFoundResult();
        }
        await UpdateCustomerAsync(customer, request, cancellationToken);
        return Result.Success(MapToReadCustomerDto(customer));
    }

    /// <summary>
    /// Validates the input. 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Result> ValidateRequestAsync(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure<ReadCustomerDto>(new Error("UpdateCustomer.Validation", validationResult.ToString()));
        }
        return Result.Success();
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
    /// Updates the customer and persists the changes in the database table. 
    /// </summary>
    /// <param name="customer"></param>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Result<Customer>> UpdateCustomerAsync(Customer customer, UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        customer.UpdateCustomerInfo(request.FirstName, request.LastName, request.Email, request.PhoneNumber, request.Address);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(customer);
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

public class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerCommandValidator()
    {
        RuleFor(c => c.FirstName).NotEmpty().When(c => c.FirstName is not null).WithMessage("First Name cannot be empty if provided.");
        RuleFor(c => c.LastName).NotEmpty().When(c => c.LastName is not null).WithMessage("Last Name cannot be empty if provided.");
        RuleFor(c => c.Email).NotEmpty().When(c => c.Email is not null).WithMessage("Email cannot be empty if provided.");
        RuleFor(c => c.PhoneNumber).NotEmpty().When(c => c.PhoneNumber is not null).WithMessage("Phone Number cannot be empty if provided.");
        RuleFor(c => c.Address).NotEmpty().When(c => c.Address is not null).WithMessage("Address cannot be empty if provided.");
    }
}
