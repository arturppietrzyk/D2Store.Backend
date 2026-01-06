namespace D2Store.Api.Features.Products.Dto;

public record ReadProductDto(Guid ProductId, string Name, string Description, decimal Price, int StockQuantity, DateTime AddedDate, DateTime LastModified, IReadOnlyCollection<ReadProductImageDto> Images, IReadOnlyCollection<ReadProductCategoryDto> Categories);

public record ReadProductImageDto (Guid ProductImageId, string Location, bool IsPrimary);

public record ReadProductCategoryDto(Guid ProductId, Guid CategoryId);

