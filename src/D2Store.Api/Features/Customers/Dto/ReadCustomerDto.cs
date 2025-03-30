namespace D2Store.Api.Features.Customers.Dto;

public record ReadCustomerDto(Guid CustomerId, string FirstName, string LastName, string Email, string PhoneNumber, string Address, DateTime CreatedAt, DateTime LastModified);

