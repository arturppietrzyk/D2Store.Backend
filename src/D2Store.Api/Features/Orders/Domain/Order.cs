namespace D2Store.Api.Features.Orders.Domain;

public class Order
{
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public DateTime OrderDate { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string Status { get; private set; }

    public Order(Guid customerId, decimal totalAmount)
    {
        Id = Guid.NewGuid();
        CustomerId = customerId;
        OrderDate = DateTime.UtcNow;
        TotalAmount = totalAmount;
        Status = "Paid";
    }

    public void UpdateTotalAmount(decimal? totalAmount)
    {
        if (totalAmount.HasValue)
        {
            TotalAmount = totalAmount.Value;
        }
    }

    public void UpdateStatus(string? status)
    {
        if (!string.IsNullOrWhiteSpace(status))
        {
            Status = status;
        }
    }
}