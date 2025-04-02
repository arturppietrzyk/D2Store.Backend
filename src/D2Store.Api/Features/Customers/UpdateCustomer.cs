using D2Store.Api.Features.Customers.Dto;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Customers;

public record UpdateCustomerCommand(Guid CustomerId, string? FirstName, string? LastName, string? Email, string? PhoneNumber, string? Address) : IRequest<Result<ReadCustomerDto>>;

public class UpdateCustomerHandler : IRequestHandler<UpdateCustomerCommand, Result<ReadCustomerDto>>
{
    private readonly AppDbContext _dbContext;
    private readonly IValidator<UpdateCustomerCommand> _validator;
    private readonly ILogger<UpdateCustomerHandler> _logger;

    public UpdateCustomerHandler(AppDbContext dbContext, IValidator<UpdateCustomerCommand> validator, ILogger<UpdateCustomerHandler> logger)
    {
        _dbContext = dbContext;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<ReadCustomerDto>> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var inputValidationResult = Result.Failure<ReadCustomerDto>(new Error("UpdateCustomer.Validation", validationResult.ToString()));
            _logger.LogWarning("{Class}: {Method} - Warning: {ErrorCode} - {ErrorMessage}.", nameof(UpdateCustomerHandler), nameof(Handle), inputValidationResult.Error.Code, inputValidationResult.Error.Message);
            return inputValidationResult;
        }
        var customer = await _dbContext.Customers.FirstOrDefaultAsync(c => c.CustomerId == request.CustomerId, cancellationToken);
        if (customer is null)
        {
            var result = Result.Failure<ReadCustomerDto>(new Error("UpdateCustomer.Validation", "Customer not found."));
            _logger.LogWarning("{Class}: {Method} - Warning: {ErrorCode} - {ErrorMessage}.", nameof(UpdateCustomerHandler), nameof(Handle), result.Error.Code, result.Error.Message);
            return result;
        }
        customer.UpdateCustomerInfo(request.FirstName, request.LastName, request.Email, request.PhoneNumber, request.Address);
        await _dbContext.SaveChangesAsync(cancellationToken);
        var updatedCustomer = new ReadCustomerDto(customer.CustomerId, customer.FirstName, customer.LastName, customer.Email, customer.PhoneNumber, customer.Address, customer.CreatedAt, customer.LastModified
        );
        _logger.LogInformation("{Class}: {Method} - Success, updated: {customerId}.", nameof(UpdateCustomerHandler), nameof(Handle), updatedCustomer.CustomerId);
        return Result.Success(updatedCustomer);
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
