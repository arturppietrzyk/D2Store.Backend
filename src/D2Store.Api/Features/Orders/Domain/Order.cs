using D2Store.Api.Features.Products.Domain;

namespace D2Store.Api.Features.Orders.Domain;

public class Order
{
    public Guid OrderId { get; private set; }
    public Guid CustomerId { get; private set; }
    private readonly List<OrderProduct> _products = new List<OrderProduct>();
    public IReadOnlyCollection<OrderProduct> Products => _products.AsReadOnly();
    public DateTime OrderDate { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string Status { get; private set; }
    public DateTime LastModified { get; private set; }

    private Order(Guid customerId, decimal totalAmount)
    {
        OrderId = Guid.CreateVersion7();
        CustomerId = customerId;
        OrderDate = DateTime.UtcNow;
        TotalAmount = totalAmount;
        Status = "Paid";
        LastModified = DateTime.UtcNow;
    }

    public static Order Create(Guid customerId, decimal totalAmount)
    {
        var order = new Order(customerId, totalAmount);
        return order;
    }

    public void AddProduct(Product product, int quantity)
    {
        var orderProduct = OrderProduct.Create(this.OrderId, product, quantity);
        _products.Add(orderProduct);
    }

    public void UpdateTotalAmount(decimal amount)
    {
        TotalAmount = amount;
        LastModified = DateTime.UtcNow;
    }
}