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

    public void UpdateProductInfo(string? name, string description, decimal? price, int? stockQuantity)
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
}
