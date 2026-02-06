namespace D2Store.Api.Features.Baskets.Dto;

public record WriteBasketDtoUpsert
{
    public required Guid UserId { get; init; }
    public required WriteBasketProductDtoCreate Product { get; init; }
}

public record WriteBasketProductDtoCreate()
{
    public required Guid ProductId { get; init; }
    public required int Quantity { get; init; }
}