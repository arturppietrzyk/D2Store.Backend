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

    public static Result<Customer> Create(string firstName, string lastName, string email, string phoneNumber, string address, bool customerExists)
    {
        if(customerExists == true) 
        {
            return Result.Failure<Customer>(new Error(
            "CreateCustomer.Validation", 
            "Customer already exists."));
        }
        var customer = new Customer(firstName, lastName, email, phoneNumber, address);
        return Result.Success(customer);
    }

    public Result Update(string? firstName, string? lastName, string? email, string? phoneNumber, string? address, bool customerWithEmailExists)
    {
        if(customerWithEmailExists == true)
        {
            return Result.Failure(new Error(
            "UpdateCustomer.Validation", 
            "Email already in use."));
        }
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
        return Result.Success();
    }

    public Result Delete(bool ordersExist)
    {
        if(ordersExist == true)
        {
            return Result.Failure(new Error(
           "DeleteCustomer.Validation", 
           "Customer cannot be deleted because they have orders."));
        }
        return Result.Success();
    }
}
