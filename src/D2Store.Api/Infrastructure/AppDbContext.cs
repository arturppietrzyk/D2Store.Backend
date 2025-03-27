using D2Store.Api.Features.Customers.Domain;
using D2Store.Api.Features.Orders.Domain;
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
}
