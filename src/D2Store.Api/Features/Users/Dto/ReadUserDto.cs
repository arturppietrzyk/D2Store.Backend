namespace D2Store.Api.Features.Users.Dto;

public record ReadUserDto(Guid UserId, string FirstName, string LastName, string Email, string PasswordHash, string PhoneNumber, string Address, string Role, DateTime CreatedDate, DateTime LastModified);

public record ReadAuthDto(string AccessToken, DateTime ExpiresAt);