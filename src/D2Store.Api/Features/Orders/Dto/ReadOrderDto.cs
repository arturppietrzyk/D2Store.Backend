namespace D2Store.Api.Features.Orders.Dto;

public record ReadOrderDto(Guid Id, Guid CustomerId, DateTime OrderDate, decimal TotalAmount, string Status);
