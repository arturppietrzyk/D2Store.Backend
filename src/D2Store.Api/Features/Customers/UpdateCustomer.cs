using D2Store.Api.Features.Customers.Dto;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Customers;

public record UpdateCustomerCommand(Guid CustomerId, string FirstName, string LastName, string Email, string PhoneNumber, string Address) : IRequest<Result<ReadCustomerDto>>;

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
            var inputValidationResult = Result.Failure<ReadCustomerDto>(new Error("Update.Validation", validationResult.ToString()));
            _logger.LogWarning("{Class}: {Method} - Warning: {ErrorCode} - {ErrorMessage}.", nameof(UpdateCustomerHandler), nameof(Handle), inputValidationResult.Error.Code, inputValidationResult.Error.Message);
            return inputValidationResult;
        }
        var customer = await _dbContext.Customers.FirstOrDefaultAsync(c => c.CustomerId == request.CustomerId, cancellationToken);
        if (customer is null)
        {
            var result = Result.Failure<ReadCustomerDto>(new Error("UpdateCustomer.NotFound", "Customer not found."));
            _logger.LogWarning("{Class}: {Method} - Warning: {ErrorCode} - {ErrorMessage}.", nameof(UpdateCustomerHandler), nameof(Handle), result.Error.Code, result.Error.Message);
            return result;
        }
        customer.UpdateCustomerInfo(request.FirstName, request.LastName, request.Email, request.PhoneNumber, request.Address);
        await _dbContext.SaveChangesAsync(cancellationToken);
        var updatedCustomer = new ReadCustomerDto(customer.CustomerId, customer.FirstName, customer.LastName, customer.Email, customer.PhoneNumber, customer.Address, customer.CreatedAt);
        _logger.LogInformation("{Class}: {Method} - Success, updated: {customerId}.", nameof(UpdateCustomerHandler), nameof(Handle), updatedCustomer.CustomerId.ToString());
        return Result.Success(updatedCustomer);
    }
}

public class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerCommandValidator()
    {
        RuleFor(c => c.FirstName).NotEmpty().WithMessage("First Name is required.");
        RuleFor(c => c.LastName).NotEmpty().WithMessage("Last Name is required.");
        RuleFor(c => c.Email).NotEmpty().WithMessage("Email is required.");
        RuleFor(c => c.PhoneNumber).NotEmpty().WithMessage("Phone Number is required.");
        RuleFor(c => c.Address).NotEmpty().WithMessage("Address is required.");
    }
}