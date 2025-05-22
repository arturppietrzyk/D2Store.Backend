using D2Store.Api.Shared;

namespace D2Store.Api.Features.Customers.Domain;

public class Customer
{
    public Guid CustomerId { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }
    public string PhoneNumber { get; private set; }
    public string Address { get; private set; }
    public DateTime CreatedDate { get; private set; }
    public DateTime LastModified { get; private set; }

    private Customer(string firstName, string lastName, string email, string phoneNumber, string address)
    {
        CustomerId = Guid.CreateVersion7();
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phoneNumber;
        Address = address;
        CreatedDate = DateTime.UtcNow;
        LastModified = DateTime.UtcNow;
    }

    public static Customer Create(string firstName, string lastName, string email, string phoneNumber, string address)
    {
        var customer = new Customer(firstName, lastName, email, phoneNumber, address);
        return customer;
    }

    public void Update(string? firstName, string? lastName, string? email, string? phoneNumber, string? address)
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
    }

    public static Result ValidateCustomerExsistance(bool customerExists)
    {
        if (!customerExists)
        {
            return Result.Failure(new Error(
                "Customer.Validation",
                "Customer does not exist."));
        }
        return Result.Success();
    }

    public static Result ValidateEmailUniqueness(bool emailInUse)
    {
        if (emailInUse)
        {
            return Result.Failure(new Error(
                "Customer.Validation", 
                "Customer email already in use."));
        }
        return Result.Success();
    }

    public static Result ValidateOrdersExistance(bool hasOrders)
    {
        if (hasOrders)
        {
            return Result.Failure(new Error(
                "Customer.Validation",
                "Customer cannot be deleted because they have orders."));
        }
        return Result.Success();
    }
}
