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
    private readonly List<ProductImage> _images = new List<ProductImage>();
    public IReadOnlyCollection<ProductImage> Images => _images.AsReadOnly();
    
    private Product(string name, string description, decimal price, int stockQuantity)
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

    public void AddImage(string location, bool isPrimary)
    {
        var productImage = ProductImage.Create(this.ProductId, location, isPrimary);
        _images.Add(productImage);
    }

    public void RemoveImages(IEnumerable<Guid> productImageIds)
    {
        _images.RemoveAll(img => productImageIds.Contains(img.ProductImageId));
    }

    public bool Update(string? name, string? description, decimal? price, int? stockQuantity)
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
        return isUpdated;
    }

    public Result ReduceStock(int quantity)
    {
        var stockCheck = AssertProductHasSufficientStock(quantity);
        if (stockCheck.IsFailure)
        {
            return stockCheck;
        }
        StockQuantity -= quantity;
        LastModified = DateTime.UtcNow;
        return Result.Success();
    }

    public Result AssertProductHasSufficientStock(int requestedQuantity)
    {
        if (StockQuantity < requestedQuantity)
        {
            return Result.Failure(new Error(
                "Product.Validation",
                $"Insufficient stock for product '{Name}'. Available: {StockQuantity}, Requested: {requestedQuantity}"));
        }
        return Result.Success();
    }

    public static Result AssertOrderProductExistance(bool hasOrderProducts)
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