namespace D2Store.Api.Features.Products.Dto;

public record WriteProductDtoCreate
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required decimal Price { get; set; }
    public required int StockQuantity { get; set; }
    public required IReadOnlyCollection<WriteProductImageDtoCreate> Images { get; set; }
}

public record WriteProductDtoUpdate
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public int? StockQuantity { get; set; }
}

public record WriteProductImagesDtoAdd
{
    public required IReadOnlyCollection<WriteProductImageDtoCreate> Images { get; set; }
}

public record WriteProductImagesDtoRemove
{
    public required IReadOnlyCollection<Guid> ProductImageIds { get; set; }
}

public record WriteProductImageDtoCreate
{
    public required string Location { get; set; }
    public required bool IsPrimary { get; set; }
}