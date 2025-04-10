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

    public UpdateOrderHandler(AppDbContext dbContext, IValidator<UpdateOrderCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async ValueTask<Result<ReadOrderDto>> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var inputValidationResult = Result.Failure<ReadOrderDto>(new Error("UpdateOrder.Validation", validationResult.ToString()));
            return inputValidationResult;
        }
        var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.OrderId == request.OrderId, cancellationToken);
        if (order is null)
        {
            var result = Result.Failure<ReadOrderDto>(new Error("UpdateOrder.Validation", "Order not found."));
            return result;
        }
        order.UpdateOrderInfo(request.TotalAmount);
        await _dbContext.SaveChangesAsync(cancellationToken);
        List<ReadOrderProductDto> placeHolder = new List<ReadOrderProductDto>();
        var updatedOrder = new ReadOrderDto(order.OrderId, order.CustomerId, placeHolder, order.OrderDate, order.TotalAmount, order.Status, order.LastModified);
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