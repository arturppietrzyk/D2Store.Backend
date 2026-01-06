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
    public DateTime LastModified { get; private set; }
    private readonly List<ProductImage> _images = new List<ProductImage>();
    public IReadOnlyCollection<ProductImage> Images => _images.AsReadOnly();
    private readonly List<ProductCategory> _categories = new List<ProductCategory>();
    public IReadOnlyCollection<ProductCategory> Categories => _categories.AsReadOnly();

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

    public Result Update(string? name, string? description, decimal? price, int? stockQuantity)
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
        if (isUpdated == true)
        {
            LastModified = DateTime.UtcNow;
            return Result.Success();
        }
        else
        {
            return Result.Failure(new Error("Product.Validation", "The changes are no different to what is currently there."));
        }
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
            return Result.Failure(new Error("Product.Validation", $"Insufficient stock for product '{Name}'. Available: {StockQuantity}, Requested: {requestedQuantity}"));
        }
        return Result.Success();
    }

    public static Result AssertOrderProductExistance(bool hasOrderProducts)
    {
        if (hasOrderProducts)
        {
            return Result.Failure(new Error("Product.Validation", "Product cannot be deleted because it's part of an order."));
        }
        return Result.Success();
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

    public Result ChangePrimaryImage(Guid productImageId)
    {
        var isUpdated = false;
        var currentPrimary = _images.SingleOrDefault(i => i.IsPrimary == true);
        if (currentPrimary is not null && currentPrimary.ProductImageId == productImageId)
        {
            return Result.Failure(new Error("Product.Validation", "The new primary image is no different to what is currently set as the primary image."));
        }
        if (currentPrimary is not null)
        {
            currentPrimary.SetPrimary(false);
            isUpdated = true;
        }
        var newPrimaryImage = _images.SingleOrDefault(i => i.ProductImageId == productImageId);
        if (newPrimaryImage is not null)
        {
            newPrimaryImage.SetPrimary(true);
            isUpdated = true;
        }
        if (isUpdated == true)
        {
            LastModified = DateTime.UtcNow;
        }
        return Result.Success();
    }

    public Result AssertProductImageBeingRemovedIsNotAPrimaryImage(IEnumerable<Guid> productImageIds)
    {
        var primaryImageBeingRemoved = _images
            .Where(i => productImageIds.Contains(i.ProductImageId))
            .Any(i => i.IsPrimary);
        if (primaryImageBeingRemoved)
        {
            return Result.Failure(new Error("Product.Validation", "Product images cannot be removed because one of the images attempted to be removed is currently set as the primary image."));
        }
        return Result.Success();
    }

    public void AddCategory(Guid categoryId)
    {
        var productCategory = ProductCategory.Create(this.ProductId, categoryId);
        _categories.Add(productCategory);
    }

    public Result AssertProductCategoriesDoNotExist(IEnumerable<Guid> incomingCategoryIds)
    {
        var duplicateExists = _categories.Any(existing => incomingCategoryIds.Contains(existing.CategoryId));
        if (duplicateExists)
        {
            return Result.Failure(new Error("Product.Validation", "One or more categories are already assigned to this product."));
        }
        return Result.Success();
    }

    public void RemoveCategories(IEnumerable<Guid> productCategoryIds)
    {
        _categories.RemoveAll(pc => productCategoryIds.Contains(pc.CategoryId));
    }
}