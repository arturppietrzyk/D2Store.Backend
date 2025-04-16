using D2Store.Api.Features.Customers.Domain;
using D2Store.Api.Features.Orders.Domain;
using D2Store.Api.Features.Products.Domain;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
    {
    }

    public DbSet<Order> Orders { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<OrderProduct> OrderProducts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>()
            .Property(o => o.TotalAmount)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Product>()
           .Property(p => p.Price)
           .HasColumnType("decimal(18,2)");


        modelBuilder.Entity<Order>()
      .HasMany(o => o.Products)
      .WithOne(op => op.Order)
      .HasForeignKey(op => op.OrderId)
      .OnDelete(DeleteBehavior.Cascade);

        base.OnModelCreating(modelBuilder);
    }
}
