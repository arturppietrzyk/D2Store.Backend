using D2Store.Api.Shared.Enums;

namespace D2Store.Api.Features.Orders.Dto;

public record WriteOrderDtoCreate
{
    public required Guid UserId { get; set; }
    public required IReadOnlyCollection<WriteOrderProductDtoCreate> Products { get; set; }
}

public record WriteOrderProductDtoCreate()
{
    public required Guid ProductId { get; set; }
    public required int Quantity { get; set; }
}

public record WriteOrderDtoUpdate
{
    public required OrderStatus Status { get; set; }
}