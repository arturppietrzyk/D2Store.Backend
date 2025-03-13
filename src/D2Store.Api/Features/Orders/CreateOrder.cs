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

    public CreateOrderHandler(AppDbContext dbContext, IValidator<CreateOrderCommand> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }
    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var validationResult = _validator.Validate(request);
        if (!validationResult.IsValid)
        {
            return Result.Failure<Guid>(new Error(
                "CreateOrder.Validation",
                validationResult.ToString()));
        }
        var order = new Order(request.CustomerId, request.TotalAmount);
        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync(cancellationToken);
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