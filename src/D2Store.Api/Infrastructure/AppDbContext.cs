using D2Store.Api.Features.Categories;
using D2Store.Api.Features.Orders.Domain;
using D2Store.Api.Features.Products.Domain;
using D2Store.Api.Features.Users.Domain;
using Microsoft.EntityFrameworkCore;

namespace D2Store.Api.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
    {
    }

    public DbSet<Order> Orders { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<OrderProduct> OrderProducts { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<ProductCategory> ProductCategories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>()
            .Property(o => o.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Order>()
            .Property(o => o.TotalAmount)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Order>()
            .HasMany(o => o.Products)
            .WithOne(o => o.Order)
            .HasForeignKey(op => op.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Order>()
            .Property(o => o.OrderId)
            .ValueGeneratedNever();

        modelBuilder.Entity<Product>()
           .Property(p => p.Price)
           .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Product>()
            .Property(p => p.ProductId)
            .ValueGeneratedNever();

        modelBuilder.Entity<Product>()
            .HasMany(p => p.Images)
            .WithOne(p => p.Product)
            .HasForeignKey(pi => pi.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .Property(u => u.UserId)
            .ValueGeneratedNever();

        modelBuilder.Entity<OrderProduct>()
            .Property(op => op.OrderProductId)
            .ValueGeneratedNever();

        modelBuilder.Entity<ProductImage>()
            .Property(pi => pi.ProductImageId)
            .ValueGeneratedNever();

        modelBuilder.Entity<Category>()
        .Property(c => c.CategoryId)
        .ValueGeneratedNever();

        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.HasKey(pc => new { pc.ProductId, pc.CategoryId });

            entity.HasOne(pc => pc.Product)
                .WithMany(p => p.Categories)
                .HasForeignKey(pc => pc.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(pc => pc.Category)
                .WithMany(c => c.Categories)
                .HasForeignKey(pc => pc.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
