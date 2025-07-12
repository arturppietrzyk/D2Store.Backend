namespace D2Store.Api.Features.Orders.Dto;

public record ReadOrderDto(Guid OrderId, Guid UserId, List<ReadOrderProductDto> Products, DateTime OrderDate, decimal TotalAmount, string Status, DateTime LastModified);

public record ReadOrderProductDto(Guid ProductId, string Name, string Description, decimal Price, int Quantity);