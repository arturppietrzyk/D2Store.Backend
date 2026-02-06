using D2Store.Api.Features.Products.Domain;
using D2Store.Api.Shared;
using D2Store.Api.Shared.Enums;

namespace D2Store.Api.Features.Orders.Domain;

public class Order
{
    public Guid OrderId { get; private set; }
    public Guid UserId { get; private set; }
    private readonly List<OrderProduct> _products = new List<OrderProduct>();
    public IReadOnlyCollection<OrderProduct> Products => _products.AsReadOnly();
    public DateTime OrderDate { get; private set; }
    public decimal TotalAmount { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime LastModified { get; private set; }

    private Order(Guid userId, decimal totalAmount)
    {
        OrderId = Guid.CreateVersion7();
        UserId = userId;
        OrderDate = DateTime.UtcNow;
        TotalAmount = totalAmount;
        Status = OrderStatus.PAID;
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

    public static decimal CalculateTotalAmount(CreateOrderCommand request, IReadOnlyDictionary<Guid, Product> productsDict)
    {
        decimal total = 0;
        foreach (var orderProduct in request.Products)
        {
            var product = productsDict[orderProduct.ProductId];
            total += product.Price * orderProduct.Quantity;
        }
        return total;
    }

    public Result Update(OrderStatus status)
    {
        bool isUpdated = false;
        if (status != Status)
        {
            Status = status;
            isUpdated = true;
        }
        if (isUpdated == true)
        {
            LastModified = DateTime.UtcNow;
            return Result.Success();
        }
        else
        {
            return Result.Failure(new Error("Order.Validation", "The changes are no different to what is currently there."));
        }
    }

    public static Result AssertUserExsistance(bool userExists)
    {
        if (!userExists)
        {
            return Result.Failure(new Error("Order.Validation", "User does not exist."));
        }
        return Result.Success();
    }

    public static Result AssertProductsExistance(CreateOrderCommand request, Dictionary<Guid, Product> productsDict)
    {
        foreach (var product in request.Products)
        {
            if (!productsDict.ContainsKey(product.ProductId))
            {
                return Result.Failure(new Error("Order.Validation", $"Product with ProductId '{product.ProductId}' does not exist."));
            }
        }
        return Result.Success();
    }

    public static Result AssertStockAvailability(CreateOrderCommand request, Dictionary<Guid, Product> productsDict)
    {
        foreach (var orderProduct in request.Products)
        {
            var product = productsDict[orderProduct.ProductId];
            var stockCheck = product.AssertProductHasSufficientStock(orderProduct.Quantity);
            if (stockCheck.IsFailure)
            {
                return stockCheck;
            }
        }
        return Result.Success();
    }
}