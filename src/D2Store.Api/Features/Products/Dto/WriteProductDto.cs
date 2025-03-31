namespace D2Store.Api.Features.Products.Dto;

public class WriteProductDtoCreate
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required decimal Price { get; set; }
    public required int StockQuantity { get; set; }
}
