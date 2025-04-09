using D2Store.Api.Features.Customers.Domain;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using FluentValidation;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Customers;

public record CreateCustomerCommand(string FirstName, string LastName, string Email, string PhoneNumber, string Address) : IRequest<Result<Guid>>;

public class CreateCustomerHandler : IRequestHandler<CreateCustomerCommand, Result<Guid>> 
{
    private readonly AppDbContext _dbContext;
    private readonly IValidator<CreateCustomerCommand> _validator;

    public CreateCustomerHandler(AppDbContext dbContext, IValidator<CreateCustomerCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async ValueTask<Result<Guid>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var inputValidationResult = Result.Failure<Guid>(new Error("CreateCustomer.Validation", validationResult.ToString()));
            return inputValidationResult;
        }
        var customerExists = await _dbContext.Customers.AnyAsync(c => c.Email == request.Email, cancellationToken);
        if (customerExists)
        {
            var result = Result.Failure<Guid>(new Error("CreateCustomer.Validation", "Customer already exist."));
            return result;
        }
        var customer = new Customer(request.FirstName, request.LastName, request.Email, request.PhoneNumber, request.Address);
        _dbContext.Customers.Add(customer);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(customer.CustomerId);
    }
}

public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(c => c.FirstName).NotEmpty().WithMessage("First Name is required.");
        RuleFor(c => c.LastName).NotEmpty().WithMessage("Last Name is required.");
        RuleFor(c => c.Email).NotEmpty().WithMessage("Email is required.");
        RuleFor(c => c.PhoneNumber).NotEmpty().WithMessage("Phone Number is required.");
        RuleFor(c => c.Address).NotEmpty().WithMessage("Address is required.");
    }
}
