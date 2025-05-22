using D2Store.Api.Features.Customers.Domain;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using FluentValidation;
using Mediator;
using Microsoft.EntityFrameworkCore;
using D2Store.Api.Features.Customers.Dto;
namespace D2Store.Api.Features.Customers;

public record CreateCustomerCommand(string FirstName, string LastName, string Email, string PhoneNumber, string Address) : IRequest<Result<ReadCustomerDto>>;

public class CreateCustomerHandler : IRequestHandler<CreateCustomerCommand, Result<ReadCustomerDto>> 
{
    private readonly AppDbContext _dbContext;
    private readonly IValidator<CreateCustomerCommand> _validator;

    public CreateCustomerHandler(AppDbContext dbContext, IValidator<CreateCustomerCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    /// <summary>
    /// Coordinates validation, mapping and creating of an customer. Returns the created customer in a response DTO.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<Result<ReadCustomerDto>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await ValidateRequestAsync(request, cancellationToken);
        if (validationResult.IsFailure)
        {
            return Result.Failure<ReadCustomerDto>(validationResult.Error);
        }
        if (request.Email is not null)
        {
            var emailInUse = await _dbContext.Customers.AsNoTracking().AnyAsync(c => c.Email == request.Email, cancellationToken);
            var validateEmailUniquenessResult = Customer.ValidateEmailUniqueness(emailInUse);
            if (validateEmailUniquenessResult.IsFailure)
            {
                return Result.Failure<ReadCustomerDto>(validateEmailUniquenessResult.Error);
            }
        }
        var createCustomer = await CreateCustomerAsync(request, cancellationToken);
        var customerDto = MapToReadCustomerDto(createCustomer);
        return Result.Success(customerDto);
    }

    /// <summary>
    /// Validates the input. 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Result> ValidateRequestAsync(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure<Customer>(new Error("CreateCustomer.Validation", validationResult.ToString()));
        }
        return Result.Success();
    }

    /// <summary>
    /// Creates the customer, persisting it to the database. 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Customer> CreateCustomerAsync(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var createCustomer = Customer.Create(request.FirstName, request.LastName, request.Email, request.PhoneNumber, request.Address);
        _dbContext.Customers.Add(createCustomer);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return createCustomer;
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
