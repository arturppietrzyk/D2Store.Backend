using D2Store.Api.Features.Orders.Dto;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using FluentValidation;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Orders;

public record UpdateOrderCommand(Guid OrderId, decimal? TotalAmount) : IRequest<Result<ReadOrderDto>>;

public class UpdateOrderHandler : IRequestHandler<UpdateOrderCommand, Result<ReadOrderDto>>
{
    private readonly AppDbContext _dbContext;
    private readonly IValidator<UpdateOrderCommand> _validator;
    private readonly ILogger<UpdateOrderHandler> _logger;

    public UpdateOrderHandler(AppDbContext dbContext, IValidator<UpdateOrderCommand> validator, ILogger<UpdateOrderHandler> logger)
    {
        _dbContext = dbContext;
        _validator = validator;
        _logger = logger;
    }

    public async ValueTask<Result<ReadOrderDto>> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var inputValidationResult = Result.Failure<ReadOrderDto>(new Error("UpdateOrder.Validation", validationResult.ToString()));
            _logger.LogWarning("{Class}: {Method} - Warning: {ErrorCode} - {ErrorMessage}.", nameof(UpdateOrderHandler), nameof(Handle), inputValidationResult.Error.Code, inputValidationResult.Error.Message);
            return inputValidationResult;
        }
        var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.OrderId == request.OrderId, cancellationToken);
        if (order is null)
        {
            var result = Result.Failure<ReadOrderDto>(new Error("UpdateOrder.Validation", "Order not found."));
            _logger.LogWarning("{Class}: {Method} - Warning: {ErrorCode} - {ErrorMessage}.", nameof(UpdateOrderHandler), nameof(Handle), result.Error.Code, result.Error.Message);
            return result;
        }
        order.UpdateOrderInfo(request.TotalAmount);
        await _dbContext.SaveChangesAsync(cancellationToken);
        var updatedOrder = new ReadOrderDto(order.OrderId, order.CustomerId, order.OrderDate, order.TotalAmount, order.Status, order.LastModified);
        _logger.LogInformation("{Class}: {Method} - Success, updated: {orderId}.", nameof(UpdateOrderHandler), nameof(Handle), updatedOrder.OrderId.ToString());
        return Result.Success(updatedOrder);
    }
}

public class UpdateOrderCommandValidator : AbstractValidator<UpdateOrderCommand>
{
    public UpdateOrderCommandValidator()
    {
        RuleFor(o => o.TotalAmount).GreaterThan(0).When(o => o.TotalAmount is not null).WithMessage("Total Amount must be greater than zero.");
    }
}