namespace D2Store.Api.Features.Products.Dto;

public class WriteProductDtoCreate
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required decimal Price { get; set; }
    public required int StockQuantity { get; set; }
    public required List<WriteProductImageDtoCreate> Images { get; set; }
}

public class WriteProductImageDtoCreate
{
    public required string Location { get; set; }
    public required bool IsPrimary { get; set; }
}

public class WriteProductImageDtoDelete
{
    public required List<Guid> ProductImageIds { get; set; }
}

public class WriteProductDtoUpdate
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public int? StockQuantity { get; set; }
}