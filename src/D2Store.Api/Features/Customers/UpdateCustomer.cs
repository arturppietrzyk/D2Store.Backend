using D2Store.Api.Features.Customers.Domain;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using FluentValidation;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Customers;

public record UpdateCustomerCommand(Guid CustomerId, string? FirstName, string? LastName, string? Email, string? PhoneNumber, string? Address) : IRequest<Result<Guid>>;

public class UpdateCustomerHandler : IRequestHandler<UpdateCustomerCommand, Result<Guid>>
{
    private readonly AppDbContext _dbContext;
    private readonly IValidator<UpdateCustomerCommand> _validator;

    public UpdateCustomerHandler(AppDbContext dbContext, IValidator<UpdateCustomerCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    /// <summary>
    /// Coordinates validation, retrieval, and updating of a specific customer. Returns the Guid of the deleted customer if successful. 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<Result<Guid>> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await ValidateRequestAsync(request, cancellationToken);
        if (validationResult.IsFailure)
        {
            return Result.Failure<Guid>(validationResult.Error);
        }
        var customer = await GetCustomerAsync(request.CustomerId, cancellationToken);
        if (customer == null)
        {
            return CustomerNotFoundResult();
        }
        var customerWithEmailExists = await _dbContext.Customers.AsNoTracking().AnyAsync(c => c.Email == request.Email, cancellationToken);
        var updateCustomerResult = await UpdateCustomerAsync(customer, request, customerWithEmailExists, cancellationToken);
        return updateCustomerResult;
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
            return Result.Failure<Guid>(new Error("UpdateCustomer.Validation", validationResult.ToString()));
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
    private static Result<Guid> CustomerNotFoundResult()
    {
        return Result.Failure<Guid>(new Error(
            "UpdateCustomer.Validation",
            "The customer with the specified Customer Id was not found."));
    }

    /// <summary>
    /// Validates the business rules, updates the customer, persisting the changes in the database table. 
    /// </summary>
    /// <param name="customer"></param>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Result<Guid>> UpdateCustomerAsync(Customer customer, UpdateCustomerCommand request, bool customerWithEmailExists, CancellationToken cancellationToken)
    {
        var updateCustomerResult = customer.Update(request.FirstName, request.LastName, request.Email, request.PhoneNumber, request.Address, customerWithEmailExists);
        if (updateCustomerResult.IsFailure)
        {
            return Result.Failure<Guid>(updateCustomerResult.Error);
        }
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(customer.CustomerId);
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
