namespace D2Store.Api.Features.Orders.Dto;

public record ReadOrderDto(Guid OrderId, Guid CustomerId, DateTime OrderDate, decimal TotalAmount, string Status, DateTime LastModified);
