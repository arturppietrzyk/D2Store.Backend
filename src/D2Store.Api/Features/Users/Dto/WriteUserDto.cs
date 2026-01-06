namespace D2Store.Api.Features.Users.Dto;

public record WriteUserDtoRegister
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
    public required string Password { get; init; }
    public required string PhoneNumber { get; init; }
    public required string Address { get; init; }
}

public record WriteUserDtoLogin
{
    public required string Email { get; init; }
    public required string Password { get; init; }
}

public record WriteUserDtoUpdate
{
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }
    public string? Address { get; init; }
}
