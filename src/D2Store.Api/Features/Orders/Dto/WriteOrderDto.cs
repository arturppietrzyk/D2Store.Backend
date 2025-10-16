using D2Store.Api.Shared.Enums;

namespace D2Store.Api.Features.Orders.Dto;

public class WriteOrderDtoCreate
{
    public required Guid UserId { get; set; }
    public required IReadOnlyCollection<WriteOrderProductDtoCreate> Products { get; set; }
}

public class WriteOrderProductDtoCreate
{
    public required Guid ProductId { get; set; }
    public required int Quantity { get; set; }
}

public class WriteOrderDtoUpdate
{
    public required OrderStatus Status { get; set; }
}