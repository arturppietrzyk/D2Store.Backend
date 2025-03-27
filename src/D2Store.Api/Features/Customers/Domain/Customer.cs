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

    public Customer(string firstName, string lastName, string email, string phoneNumber, string address)
    {
        CustomerId = Guid.CreateVersion7();
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phoneNumber;
        Address = address;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateCustomerInfo(string? firstname, string? lastName, string? email, string? phoneNumber, string? address)
    {
        if (!string.IsNullOrWhiteSpace(firstname))
        {
            FirstName = firstname;
        }
        if (!string.IsNullOrWhiteSpace(lastName))
        {
            LastName = lastName;
        }
        if (!string.IsNullOrWhiteSpace(email))
        {
            Email = email;
        }
        if (!string.IsNullOrWhiteSpace(phoneNumber))
        {
            PhoneNumber = phoneNumber;
        }
        if (!string.IsNullOrWhiteSpace(address))
        {
            Address = address;
        }
    }
}
