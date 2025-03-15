using D2Store.Api.Features.Orders.Dto;
using D2Store.Api.Infrastructure;
using D2Store.Api.Shared;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Features.Orders;

public record UpdateOrderCommand(Guid Id, decimal? TotalAmount, string? Status) : IRequest<Result<ReadOrderDto>>;

public class UpdateOrderHandler : IRequestHandler<UpdateOrderCommand, Result<ReadOrderDto>>
{
    private readonly AppDbContext _dbContext;
    private readonly IValidator<UpdateOrderCommand> _validator;

    public UpdateOrderHandler(AppDbContext dbContext, IValidator<UpdateOrderCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<Result<ReadOrderDto>> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure<ReadOrderDto>(new Error(
                "Update.Validation",
                validationResult.ToString()));
        }
        var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);
        if (order is null)
        {
            return Result.Failure<ReadOrderDto>(new Error("UpdateOrder.NotFound", "Order not found."));
        }
        order.UpdateTotalAmount(request.TotalAmount);
        order.UpdateStatus(request.Status);
        await _dbContext.SaveChangesAsync(cancellationToken);
        var updatedOrder = new ReadOrderDto(order.Id, order.CustomerId, order.OrderDate, order.TotalAmount, order.Status);
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