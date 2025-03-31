using D2Store.Api.Features.Orders.Domain;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Orders;

public record CreateOrderCommand(Guid CustomerId, decimal TotalAmount) : IRequest<Result<Guid>>;

public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, Result<Guid>>
{
    private readonly AppDbContext _dbContext;
    private readonly IValidator<CreateOrderCommand> _validator;
    private readonly ILogger<CreateOrderHandler> _logger;

    public CreateOrderHandler(AppDbContext dbContext, IValidator<CreateOrderCommand> validator, ILogger<CreateOrderHandler> logger)
    {
        _dbContext = dbContext;
        _validator = validator;
        _logger = logger;
    }
    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var inputValidationResult = Result.Failure<Guid>(new Error("CreateOrder.Validation",validationResult.ToString()));
            _logger.LogWarning("{Class}: {Method} - Warning: {ErrorCode} - {ErrorMessage}.", nameof(CreateOrderHandler), nameof(Handle), inputValidationResult.Error.Code, inputValidationResult.Error.Message);
            return inputValidationResult;
        }
        var customerExists = await _dbContext.Customers.AnyAsync(c => c.CustomerId == request.CustomerId, cancellationToken);
        if (!customerExists)
        {
            var result = Result.Failure<Guid>(new Error("CreateOrder.Validation", "Customer does not exist."));
            _logger.LogWarning("{Class}: {Method} - Warning: {ErrorCode} - {ErrorMessage}.", nameof(CreateOrderHandler), nameof(Handle), result.Error.Code, result.Error.Message);
            return result;
        }
        var order = new Order(request.CustomerId, request.TotalAmount);
        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("{Class}: {Method} - Success, created: {orderId}.", nameof(CreateOrderHandler), nameof(Handle), order.OrderId.ToString());
        return Result.Success(order.OrderId);
    }
}

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(c => c.CustomerId).NotEmpty().WithMessage("CustomerId is required.");
        RuleFor(c => c.TotalAmount).NotEmpty().WithMessage("Total Amount is required.");
        RuleFor(c => c.TotalAmount).GreaterThan(0).WithMessage("Total Amount must be greater than zero.");
    }
}