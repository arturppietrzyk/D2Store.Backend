using D2Store.Api.Features.Products.Domain;
using D2Store.Api.Shared;

namespace D2Store.Api.Features.Categories;

public class Category
{
    public Guid CategoryId { get; private set; }
    public string Name { get; private set; }
    public DateTime AddedDate { get; private set; }
    public DateTime LastModified { get; private set;}
    public IReadOnlyCollection<ProductCategory> Categories { get; set; } = new List<ProductCategory>();

    private Category(string name)
    {
        CategoryId = Guid.CreateVersion7();
        Name = name;
        AddedDate = DateTime.UtcNow;
        LastModified = DateTime.UtcNow;
    }

    public static Category Create(string name)
    {
        var category = new Category(name);
        return category;
    }

    public Result Update(string? name)
    {
        bool isUpdated = false;
        if(!string.IsNullOrEmpty(name) && name != Name)
        {
            Name = name;
            isUpdated = true;
        }
        if(isUpdated == true)
        {
            LastModified = DateTime.UtcNow;
            return Result.Success();
        }
        else
        {
            return Result.Failure(new Error("Category.Validation", "The changes are no different to what is currently there."));
        }
    }
}
