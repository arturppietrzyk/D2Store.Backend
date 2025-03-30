namespace D2Store.Api.Features.Orders.Dto;

public class WriteOrderDtoCreate
{
    public Guid CustomerId { get; set; }
    public decimal TotalAmount { get; set; }
}

public class WriteOrderDtoUpdate
{
    public decimal? TotalAmount { get; set; }
}
//test