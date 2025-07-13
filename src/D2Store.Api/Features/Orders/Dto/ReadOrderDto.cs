using D2Store.Api.Shared.Enums;

namespace D2Store.Api.Features.Orders.Dto;

public record ReadOrderDto(Guid OrderId, Guid UserId, List<ReadOrderProductDto> Products, DateTime OrderDate, decimal TotalAmount, OrderStatus Status, DateTime LastModified);

public record ReadOrderProductDto(Guid ProductId, string Name, string Description, decimal Price, int Quantity);