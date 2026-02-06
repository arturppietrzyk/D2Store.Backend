namespace D2Store.Api.Features.Baskets.Dto;

public record ReadBasketDto(Guid BasketId, Guid UserId, IReadOnlyCollection<ReadBasketProductDto> Products, DateTime CreatedAt, decimal TotalAmount, DateTime LastModified);
public record ReadBasketProductDto(Guid ProductId, string Name, string Description, decimal Price, int Quantity, Guid ProductImageId, string Location);