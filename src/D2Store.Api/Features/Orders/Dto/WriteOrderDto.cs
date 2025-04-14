namespace D2Store.Api.Features.Orders.Dto;

public class WriteOrderDtoCreate
{
    public required Guid CustomerId { get; set; }
    public required List<WriteOrderProductDtoCreate> Products { get; set; }
}

public class WriteOrderProductDtoCreate
{
    public required Guid ProductId { get; set; }
    public required int Quantity { get; set; }
}

public class WriteOrderDtoUpdate
{
    public string? Status { get; set; }
}