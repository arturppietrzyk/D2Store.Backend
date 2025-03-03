namespace D2Store.Api.Features.Products.Domain;
public class Product
{
    public Guid Id { get; set; }  // Immutable ID
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }

    // Constructor to initialize a new product
    //public Product(Guid id, string name, decimal price, int stockQuantity)
    //{
    //    Id = id;
    //    Name = name;
    //    Price = price;
    //    StockQuantity = stockQuantity;
    //}

    //// Example of domain logic you might want to add later
    //public void DecreaseStock(int quantity)
    //{
    //    if (StockQuantity >= quantity)
    //    {
    //        StockQuantity -= quantity;
    //    }
    //    else
    //    {
    //        // Throw an exception or return an error if insufficient stock
    //        throw new InvalidOperationException("Not enough stock.");
    //    }
    //}

    //public void IncreaseStock(int quantity)
    //{
    //    StockQuantity += quantity;
    //}
}
