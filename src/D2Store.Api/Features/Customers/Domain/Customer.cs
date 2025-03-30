namespace D2Store.Api.Features.Customers.Domain;

public class Customer
{
    public Guid CustomerId { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }
    public string PhoneNumber { get; private set; }
    public string Address { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime LastModified {  get; private set; }

    public Customer(string firstName, string lastName, string email, string phoneNumber, string address)
    {
        CustomerId = Guid.CreateVersion7();
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phoneNumber;
        Address = address;
        CreatedAt = DateTime.UtcNow;
        LastModified = DateTime.UtcNow;
    }

    public void UpdateCustomerInfo(string? firstName, string? lastName, string? email, string? phoneNumber, string? address)
    {
        bool isUpdated = false;
        if (!string.IsNullOrWhiteSpace(firstName) && firstName != FirstName)
        {
            FirstName = firstName;
            isUpdated = true;
        }
        if (!string.IsNullOrWhiteSpace(lastName) && lastName != LastName)
        {
            LastName = lastName;
            isUpdated = true;
        }
        if (!string.IsNullOrWhiteSpace(email) && email != Email)
        {
            Email = email;
            isUpdated = true;
        }
        if (!string.IsNullOrWhiteSpace(phoneNumber) && phoneNumber != PhoneNumber)
        {
            PhoneNumber = phoneNumber;
            isUpdated = true;
        }
        if (!string.IsNullOrWhiteSpace(address) && address != Address)
        {
            Address = address;
            isUpdated = true;
        }
        if (isUpdated)
        {
            LastModified = DateTime.UtcNow;
        }
    }
}
