namespace D2Store.Api.Features.Orders.Domain;

public class Order
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; }

    //private Order() { }

    //public Order(Guid id, string customerId, decimal totalAmount)
    //{
    //    if (string.IsNullOrWhiteSpace(customerId))
    //        throw new ArgumentException("Customer ID is required.");
    //    if (totalAmount <= 0)
    //        throw new InvalidOperationException("Total amount must be greater than zero.");

    //    Id = Guid.NewGuid();
    //    CustomerId = customerId;
    //    OrderDate = DateTime.UtcNow;
    //    TotalAmount = totalAmount;
    //    Status = "Pending";

    //}
    //public void MarkAsPaid()
    //{
    //    if (Status == "Paid")
    //        throw new InvalidOperationException("Order is already paid.");
    //    Status = "Paid";
    //}
}
