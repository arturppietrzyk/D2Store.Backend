using D2Store.Api.Features.Products.Domain;
using D2Store.Api.Shared;

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

    public static Result<Order> Create(Guid customerId, decimal totalAmount)
    {
        var order = new Order(customerId, totalAmount);
        return Result.Success(order);
    }

    public Result AddProduct(Product product, int quantity)
    {
        var orderProductResult = OrderProduct.Create(this.OrderId, product, quantity);
        if (orderProductResult.IsFailure)
        {
            return Result.Failure(orderProductResult.Error);
        }
        _products.Add(orderProductResult.Value);
        return Result.Success(orderProductResult.Value);
    }


    public void UpdateTotalAmount(decimal amount)
    {
        TotalAmount = amount;
        LastModified = DateTime.UtcNow;
    }
}