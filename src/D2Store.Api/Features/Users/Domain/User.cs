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

    public bool Update(string? firstName, string? lastName, string? email, string? phoneNumber, string? address)
    {
        bool isUpdated = false;
        if (!string.IsNullOrEmpty(firstName) && firstName != FirstName)
        {
            FirstName = firstName;
            isUpdated = true;
        }
        if (!string.IsNullOrEmpty(lastName) && lastName != LastName)
        {
            LastName = lastName;
            isUpdated = true;
        }
        if (!string.IsNullOrEmpty(email) && email != Email)
        {
            Email = email;
            isUpdated = true;
        }
        if (!string.IsNullOrEmpty(phoneNumber) && phoneNumber != PhoneNumber)
        {
            PhoneNumber = phoneNumber;
            isUpdated = true;
        }
        if (!string.IsNullOrEmpty(address) && address != Address)
        {
            Address = address;
            isUpdated = true;
        }
        if (isUpdated)
        {
            LastModified = DateTime.UtcNow;
        }
        return isUpdated;
    }

    public static Result AssertUserEmailIsUnique(bool emailInUse)
    {
        if (emailInUse)
        {
            return Result.Failure(new Error(
                "User.Validation",
                "User email already in use."));
        }
        return Result.Success();
    }

    public static Result AssertUserHasNoOrders(bool hasOrders)
    {
        if (hasOrders)
        {
            return Result.Failure(new Error(
                "User.Validation",
                "User cannot be deleted because they have orders."));
        }
        return Result.Success();
    }
}