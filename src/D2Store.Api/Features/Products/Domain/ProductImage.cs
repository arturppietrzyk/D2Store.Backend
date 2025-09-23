namespace D2Store.Api.Features.Products.Domain;

public class ProductImage
{
    public Guid ProductImageId { get; private set; }
    public Guid ProductId { get; private set; }
    public string Location { get; private set; }
    public bool IsPrimary { get; private set; }
    public Product Product { get; private set; } = null!;

    private ProductImage(Guid productId, string location, bool isPrimary)
    {
        ProductImageId = Guid.CreateVersion7();
        ProductId = productId;
        Location = location;
        IsPrimary = isPrimary;
    }

    public static ProductImage Create(Guid productId, string location, bool isPrimary)
    {
        var productImage = new ProductImage(productId, location, isPrimary);
        return productImage;
    }
}