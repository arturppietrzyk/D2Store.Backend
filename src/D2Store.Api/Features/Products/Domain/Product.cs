using D2Store.Api.Features.Orders;
using D2Store.Api.Shared;

namespace D2Store.Api.Features.Products.Domain;

public class Product
{
    public Guid ProductId { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public decimal Price { get; private set; }
    public int StockQuantity { get; private set; }
    public DateTime AddedDate { get; private set; }
    public DateTime LastModified {  get; private set; }

    public Product(string name, string description, decimal price, int stockQuantity)
    {
        ProductId = Guid.CreateVersion7();
        Name = name;
        Description = description;
        Price = price;
        StockQuantity = stockQuantity;
        AddedDate = DateTime.UtcNow;
        LastModified = DateTime.UtcNow;
    }

    public static Product Create(string name, string description, decimal price, int stockQuantity)
    {
        var product = new Product(name, description, price, stockQuantity);
        return product;
    }

    public void Update(string? name, string? description, decimal? price, int? stockQuantity)
    {
        bool isUpdated = false;
        if (!string.IsNullOrEmpty(name) && name != Name)
        {
            Name = name;
            isUpdated = true;
        }
        if (!string.IsNullOrEmpty(description) && description != Description)
        {
            Description = description;
            isUpdated = true;
        }
        if (price.HasValue && price != Price)
        {
            Price = price.Value;
            isUpdated = true;
        }
        if (stockQuantity.HasValue && stockQuantity != StockQuantity)
        {
            StockQuantity = stockQuantity.Value;
            isUpdated = true;
        }
        if (isUpdated)
        {
            LastModified = DateTime.UtcNow;
        }
    }

    public static Result ValidateProductsExistance(CreateOrderCommand request, Dictionary<Guid, Product> productsDict)
    {
        foreach (var product in request.Products)
        {
            if (!productsDict.ContainsKey(product.ProductId))
            {
                return Result.Failure(new Error(
                    "Product.Validation",
                    $"Product with ID '{product.ProductId}' does not exist."));
            }
        }
        return Result.Success();
    }

    public static Result ValidateStockAvailability(CreateOrderCommand request, Dictionary<Guid, Product> productsDict)
    {
        foreach (var orderProduct in request.Products)
        {
            var product = productsDict[orderProduct.ProductId];
            var stockCheck = product.HasSufficientStock(orderProduct.Quantity);
            if (stockCheck.IsFailure)
            {
                return stockCheck;
            }
        }
        return Result.Success();
    }

    public Result ReduceStock(int quantity)
    {
        var stockCheck = HasSufficientStock(quantity);
        if (stockCheck.IsFailure)
        {
            return stockCheck;
        }
        StockQuantity -= quantity;
        LastModified = DateTime.UtcNow;
        return Result.Success();
    }

    public Result HasSufficientStock(int requestedQuantity)
    {
        if (StockQuantity < requestedQuantity)
        {
            return Result.Failure(new Error(
                "Product.Validation",
                $"Insufficient stock for product '{Name}'. Available: {StockQuantity}, Requested: {requestedQuantity}"));
        }
        return Result.Success();
    }

    public static Result ValidateOrderProductExistance(bool hasOrderProducts)
    {
        if (hasOrderProducts)
        {
            return Result.Failure(new Error(
             "Product.Validation",
             "Product cannot be deleted because it's part of an order."));
        }
        return Result.Success();
    }
}
