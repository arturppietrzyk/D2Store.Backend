namespace D2Store.Api.Features.Orders.Dto;

public class WriteOrderDto
{
    public decimal TotalAmount { get; set; }
    public string? Status { get; set; }
}
