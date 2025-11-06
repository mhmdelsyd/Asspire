using Asspire.OrderService.Models;
using Microsoft.EntityFrameworkCore;

namespace Asspire.OrderService.Data;

public class OrderReadDbContext : DbContext
{
    public OrderReadDbContext(DbContextOptions<OrderReadDbContext> options)
        : base(options)
    {
    }

    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProductName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.CustomerName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.CustomerEmail).IsRequired().HasMaxLength(200);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
            entity.Property(e => e.TotalPrice).HasPrecision(18, 2);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CustomerEmail);
        });
    }
}
