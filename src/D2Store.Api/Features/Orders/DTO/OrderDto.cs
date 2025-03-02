namespace D2Store.Api.Features.Orders.DTO;

public class OrderDto
{
    public Guid Id { get; set; }
    public string CustomerId { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; }
}
