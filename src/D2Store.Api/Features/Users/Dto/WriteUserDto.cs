namespace D2Store.Api.Features.Users.Dto;

public class WriteUserDtoRegister
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string PhoneNumber { get; set; }
    public required string Address { get; set; }
}

public class WriteUserDtoLogin
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}

public class WriteUserDtoUpdate
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
}
