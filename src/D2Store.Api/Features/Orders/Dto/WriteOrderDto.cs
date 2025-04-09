namespace D2Store.Api.Features.Orders.Dto;

public class WriteOrderDtoCreate
{
    public Guid CustomerId { get; set; }
    public required List<WriteProductOrderDtoCreate> Products { get; set; }
}

public class WriteProductOrderDtoCreate
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

public class WriteOrderDtoUpdate
{
    public decimal? TotalAmount { get; set; }
}