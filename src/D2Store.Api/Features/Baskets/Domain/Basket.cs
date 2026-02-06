using D2Store.Api.Features.Products.Domain;
using D2Store.Api.Shared;

namespace D2Store.Api.Features.Baskets.Domain;

public class Basket
{
    public Guid BasketId { get; private set; }
    public Guid UserId { get; private set; }
    private readonly List<BasketProduct> _products = new List<BasketProduct>();
    public IReadOnlyCollection<BasketProduct> Products => _products.AsReadOnly();
    public DateTime CreatedAt { get; private set; }
    public decimal TotalAmount { get; private set; }
    public DateTime LastModified { get; private set; }

    private Basket(Guid userId)
    {
        BasketId = Guid.CreateVersion7();
        UserId = userId;
        CreatedAt = DateTime.UtcNow;
        TotalAmount = 0;
        LastModified = DateTime.UtcNow;
    }

    public static Basket Create(Guid userId)
    {
        var basket = new Basket(userId);
        return basket;
    }

    public void AddProduct(Product product, int quantity)
    {
        var basketProduct = BasketProduct.Create(this.BasketId, product, quantity);
        _products.Add(basketProduct);
        UpdateTotalAmount(product.Price, quantity);
    }

    public void UpdateExistingProductQuantity(Product product, int additionalQuantity)
    {
        UpdateTotalAmount(product.Price, additionalQuantity);
        LastModified = DateTime.UtcNow;
    }

    private void UpdateTotalAmount(decimal price, int quantity)
    {
        TotalAmount += price * quantity;
    }

    public static Result AssertUserExsistance(bool customerExists)
    {
        if (!customerExists)
        {
            return Result.Failure(new Error("Basket.Validation", "User does not exist."));
        }
        return Result.Success();
    }

    public static Result AssertProductsExistance(bool productExists)
    {
        if (!productExists)
        {
            return Result.Failure(new Error("Basket.Validation", "Product does not exist."));
        }
        return Result.Success();
    }

    public static Result AssertStockAvailability(UpsertBasketCommand request, Product product)
    {
        var stockCheck = product.AssertProductHasSufficientStock(request.Product.Quantity);
        if (stockCheck.IsFailure)
        {
            return stockCheck;
        }
        return Result.Success();
    }
}