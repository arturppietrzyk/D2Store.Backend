namespace D2Store.Api.Features.Products.Dto;

public record WriteProductDtoCreate
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required decimal Price { get; init; }
    public required int StockQuantity { get; init; }
    public required IReadOnlyCollection<WriteProductImageDtoCreate> Images { get; init; }
    public IReadOnlyCollection<WriteProductCategoryDtoCreate>? Categories { get; init; }
}

public record WriteProductDtoUpdate
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public decimal? Price { get; init; }
    public int? StockQuantity { get; init; }
}

public record WriteProductImagesDtoAdd
{
    public required IReadOnlyCollection<WriteProductImageDtoCreate> Images { get; init; }
}

public record WriteProductImagesDtoRemove
{
    public required IReadOnlyCollection<Guid> ProductImageIds { get; init; }
}

public record WriteProductImageDtoCreate
{
    public required string Location { get; init; }
    public required bool IsPrimary { get; init; }
}

public record WriteProductCategoriesDtoAdd
{
    public required IReadOnlyCollection<WriteProductCategoryDtoCreate> Categories { get; init; }
}

public record WriteProductCategoriesDtoRemove
{
    public required IReadOnlyCollection<Guid> ProductCategoryIds { get; init; }
}

public record WriteProductCategoryDtoCreate
{
    public Guid CategoryId { get; init; }
}