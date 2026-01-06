using D2Store.Api.Features.Categories;

namespace D2Store.Api.Features.Products.Domain;

public class ProductCategory
{
    public Guid ProductId  { get; private set; }
    public Guid CategoryId { get; private set;}
    public Product Product { get; private set; } = null!;
    public Category Category {get; private set;} = null!;

    private ProductCategory(Guid productId, Guid categoryId)
    {
        ProductId = productId;
        CategoryId = categoryId;
    }

    public static ProductCategory Create(Guid productId, Guid categoryId)
    {
        var productCategory = new ProductCategory(productId, categoryId);
        return productCategory;
    }
}