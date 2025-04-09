namespace D2Store.Api.Features.Orders.Domain;

public class OrderProduct
{
    public Guid OrderProductId { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public DateTime LastModified { get; private set; }

    public OrderProduct(Guid orderId, Guid productId, int quantity)
    {
        OrderProductId = Guid.CreateVersion7();
        OrderId = orderId;
        ProductId = productId;
        Quantity = quantity;
        LastModified = DateTime.UtcNow;
    }
}
