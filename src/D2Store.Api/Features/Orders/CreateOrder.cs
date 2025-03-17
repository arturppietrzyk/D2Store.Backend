using D2Store.Api.Features.Orders.Domain;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using FluentValidation;
using MediatR;

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
            var result = Result.Failure<Guid>(new Error("CreateOrder.Validation",validationResult.ToString()));
            _logger.LogWarning("{Class}: {Method} - Warning: {ErrorCode} - {ErrorMessage}.", nameof(CreateOrderHandler), nameof(Handle), result.Error.Code, result.Error.Message);
            return result;
        }
        var order = new Order(request.CustomerId, request.TotalAmount);
        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("{Class}: {Method} - Success, created: {orderId}.", nameof(CreateOrderHandler), nameof(Handle), order.Id.ToString());
        return Result.Success(order.Id);
    }
}

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        //RuleFor(c => c.CustomerId)
        //    .NotEmpty().WithMessage("CustomerId is required.");  to add later.
        RuleFor(o => o.TotalAmount)
            .GreaterThan(0).WithMessage("TotalAmount must be greater than zero.");
    }
}