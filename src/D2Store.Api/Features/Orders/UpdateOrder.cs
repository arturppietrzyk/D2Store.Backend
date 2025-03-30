using D2Store.Api.Features.Orders.Dto;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Orders;

public record UpdateOrderCommand(Guid OrderId, decimal? TotalAmount, string? Status) : IRequest<Result<ReadOrderDto>>;

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

    public async Task<Result<ReadOrderDto>> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var inputValidationResult = Result.Failure<ReadOrderDto>(new Error("Update.Validation",validationResult.ToString()));
            _logger.LogWarning("{Class}: {Method} - Warning: {ErrorCode} - {ErrorMessage}.", nameof(UpdateOrderHandler), nameof(Handle), inputValidationResult.Error.Code, inputValidationResult.Error.Message);
            return inputValidationResult;
        }
        var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.OrderId == request.OrderId, cancellationToken);
        if (order is null)
        {
            var result = Result.Failure<ReadOrderDto>(new Error("UpdateOrder.NotFound", "Order not found."));
            _logger.LogWarning("{Class}: {Method} - Warning: {ErrorCode} - {ErrorMessage}.", nameof(UpdateOrderHandler), nameof(Handle), result.Error.Code, result.Error.Message);
            return result;
        }
        order.UpdateTotalAmount(request.TotalAmount);
        order.UpdateStatus(request.Status);
        await _dbContext.SaveChangesAsync(cancellationToken);
        var updatedOrder = new ReadOrderDto(order.OrderId, order.CustomerId, order.OrderDate, order.TotalAmount, order.Status, order.LastModified);
        _logger.LogInformation("{Class}: {Method} - Success, updated: {orderId}.", nameof(UpdateOrderHandler), nameof(Handle), updatedOrder.OrderId.ToString());
        return Result.Success(updatedOrder);
    }
}

public class UpdateOrderCommandValidator : AbstractValidator<UpdateOrderCommand>
{
    private static readonly string[] AllowedStatuses = { "Pending", "Paid", "Cancelled" };

    public UpdateOrderCommandValidator()
    {
        RuleFor(o => o.TotalAmount).GreaterThan(0).WithMessage("TotalAmount must be greater than zero.");
        RuleFor(x => x.Status).Must(status => AllowedStatuses.Contains(status)).WithMessage("Invalid status. Must be either Pending, Paid, or Cancelled.");
    }
}