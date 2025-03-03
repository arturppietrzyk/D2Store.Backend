namespace D2Store.Api.Features.Customers.Domain;

public class Customer
{
    public Guid Id { get; set; }  
    public string Name { get; set; }
    public string Email { get; set; }
    public string Address { get; set; }

    // For the sake of simplicity, I'm assuming a customer might just have these properties
    // Add methods here as business logic requirements grow

    //public Customer(Guid id, string name, string email, string address)
    //{
    //    Id = id;
    //    Name = name;
    //    Email = email;
    //    Address = address;
    //}

    //// Example method: This could later become a domain method if you need customer-specific logic
    //public void UpdateEmail(string newEmail)
    //{
    //    // Add domain logic to validate the new email, if needed
    //    Email = newEmail;
    //}
}