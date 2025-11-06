using Asspire.ProductService.Models;
using Microsoft.EntityFrameworkCore;

namespace Asspire.ProductService.Data;

public class ProductWriteDbContext : DbContext
{
    public ProductWriteDbContext(DbContextOptions<ProductWriteDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.HasIndex(e => e.Category);
        });
    }
}
