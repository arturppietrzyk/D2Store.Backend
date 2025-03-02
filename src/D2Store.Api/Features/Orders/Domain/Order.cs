namespace D2Store.Api.Features.Orders.Domain;

public class Order
{
    public Guid Id { get; private set; }
    public string CustomerId { get; private set; }
    public DateTime OrderDate { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string Status { get; private set; }

    private Order() { }

    public Order(string customerId, decimal totalAmount)
    {
        if (string.IsNullOrWhiteSpace(customerId))
            throw new ArgumentException("Customer ID is required.");
        if (totalAmount <= 0)
            throw new InvalidOperationException("Total amount must be greater than zero.");

        Id = Guid.NewGuid();
        CustomerId = customerId;
        OrderDate = DateTime.UtcNow;
        TotalAmount = totalAmount;
        Status = "Pending";

    }
    public void MarkAsPaid()
    {
        if (Status == "Paid")
            throw new InvalidOperationException("Order is already paid.");
        Status = "Paid";
    }
}
