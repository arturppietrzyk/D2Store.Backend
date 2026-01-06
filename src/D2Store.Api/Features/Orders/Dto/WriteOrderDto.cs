using D2Store.Api.Shared.Enums;

namespace D2Store.Api.Features.Orders.Dto;

public record WriteOrderDtoCreate
{
    public required Guid UserId { get; init; }
    public required IReadOnlyCollection<WriteOrderProductDtoCreate> Products { get; init; }
}

public record WriteOrderProductDtoCreate()
{
    public required Guid ProductId { get; init; }
    public required int Quantity { get; init; }
}

public record WriteOrderDtoUpdate
{
    public required OrderStatus Status { get; init; }
}