namespace D2Store.Api.Features.Products.Dto;

public record ReadProductDto(Guid ProductId, string Name, string Description, decimal Price, int StockQuantity, DateTime AddedDate, DateTime LastModified);

