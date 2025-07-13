using D2Store.Api.Shared.Enums;

namespace D2Store.Api.Features.Orders.Dto;

public class WriteOrderDtoCreate
{
    public required Guid UserId { get; set; }
    public required List<WriteOrderProductDtoCreate> Products { get; set; }
}

public class WriteOrderProductDtoCreate
{
    public required Guid ProductId { get; set; }
    public required int Quantity { get; set; }
}

public class WriteOrderDtoUpdate
{
    public OrderStatus? Status { get; set; }
}