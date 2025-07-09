using D2Store.Api.Shared;

namespace D2Store.Api.Features.Users.Domain;

public class User
{
    public Guid UserId { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string PhoneNumber { get; private set; }
    public string Address { get; private set; }
    public string Role { get; private set; }
    public DateTime CreatedDate { get; private set; }
    public DateTime LastModified { get; private set; }

    private User(string firstName, string lastName, string email, string passwordHash, string phoneNumber, string address)
    {
        UserId = Guid.CreateVersion7();
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PasswordHash = passwordHash;
        PhoneNumber = phoneNumber;
        Address = address;
        Role = "CUSTOMER";
        CreatedDate = DateTime.UtcNow;
        LastModified = DateTime.UtcNow;
    }

    public static User Register(string firstName, string lastName, string email, string passwordHash, string phoneNumber, string address)
    {
        var user = new User(firstName, lastName, email, passwordHash, phoneNumber, address);
        return user;
    }

    public static Result ValidateEmailUniqueness(bool emailInUse)
    {
        if (emailInUse)
        {
            return Result.Failure(new Error(
                "User.Validation",
                "User email already in use."));
        }
        return Result.Success();
    }
}
