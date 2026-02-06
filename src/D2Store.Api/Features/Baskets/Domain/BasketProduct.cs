using D2Store.Api.Features.Products.Domain;

namespace D2Store.Api.Features.Baskets.Domain;

public class BasketProduct
{
    public Guid BasketProductId { get; private set; }
    public Guid BasketId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public DateTime LastModified { get; private set; }
    public Product Product { get; private set; } = null!;
    public Basket Basket { get; private set; } = null!;

    private BasketProduct(Guid basketId, Guid productId, int quantity)
    {
        BasketProductId = Guid.CreateVersion7();
        BasketId = basketId;
        ProductId = productId;
        Quantity = quantity;
        LastModified = DateTime.UtcNow;
    }

    public static BasketProduct Create(Guid basketId, Product product, int quantity)
    {
        var basketProduct = new BasketProduct(basketId, product.ProductId, quantity)
        {
            Product = product
        };
        return basketProduct;
    }

    public void UpdateQuantity(int newQuantity)
    {
        Quantity = newQuantity;
        LastModified = DateTime.UtcNow;
    }

}