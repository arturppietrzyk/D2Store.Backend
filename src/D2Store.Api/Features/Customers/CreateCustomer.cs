using D2Store.Api.Features.Customers.Domain;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Customers;

public record CreateCustomerCommand(string FirstName, string LastName, string Email, string PhoneNumber, string Address) : IRequest<Result<Guid>>;

public class CreateCustomerHandler : IRequestHandler<CreateCustomerCommand, Result<Guid>> 
{
    private readonly AppDbContext _dbContext;
    private readonly IValidator<CreateCustomerCommand> _validator;
    private readonly ILogger<CreateCustomerHandler> _logger;

    public CreateCustomerHandler(AppDbContext dbContext, IValidator<CreateCustomerCommand> validator, ILogger<CreateCustomerHandler> logger)
    {
        _dbContext = dbContext;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var result = Result.Failure<Guid>(new Error("CreateCustomer.Validation", validationResult.ToString()));
            _logger.LogWarning("{Class}: {Method} - Warning: {ErrorCode} - {ErrorMessage}.", nameof(CreateCustomerHandler), nameof(Handle), result.Error.Code, result.Error.Message);
            return result;
        }
        var customerExists = await _dbContext.Customers.AnyAsync(c => c.Email == request.Email, cancellationToken);
        if (customerExists)
        {
            var result = Result.Failure<Guid>(new Error("CreateCustomer.Validation", "Customer already exist."));
            _logger.LogWarning("{Class}: {Method} - Warning: {ErrorCode} - {ErrorMessage}.", nameof(CreateCustomerHandler), nameof(Handle), result.Error.Code, result.Error.Message);
            return result;
        }
        var customer = new Customer(request.FirstName, request.LastName, request.Email, request.PhoneNumber, request.Address);
        _dbContext.Customers.Add(customer);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("{Class}: {Method} - Success, created: {customerId}.", nameof(CreateCustomerHandler), nameof(Handle), customer.CustomerId.ToString());
        return Result.Success(customer.CustomerId);
    }
}

public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(c => c.FirstName).NotEmpty().WithMessage("First Name is required.");
        RuleFor(o => o.LastName).NotEmpty().WithMessage("Last Name is required.");
        RuleFor(o => o.Email).NotEmpty().WithMessage("Email is required.");
        RuleFor(o => o.PhoneNumber).NotEmpty().WithMessage("Phone Number is required.");
        RuleFor(o => o.Address).NotEmpty().WithMessage("Address is required.");
    }
}
