using Asspire.ProductService.Models;
using Microsoft.EntityFrameworkCore;

namespace Asspire.ProductService.Data;

public class DatabaseSeeder
{
    private readonly ProductWriteDbContext _writeContext;
    private readonly ProductReadDbContext _readContext;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(
        ProductWriteDbContext writeContext,
        ProductReadDbContext readContext,
        ILogger<DatabaseSeeder> logger)
    {
        _writeContext = writeContext;
        _readContext = readContext;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            // Ensure databases are created
            await _writeContext.Database.EnsureCreatedAsync();
            await _readContext.Database.EnsureCreatedAsync();

            // Check if products already exist
            if (await _writeContext.Products.AnyAsync())
            {
                _logger.LogInformation("Database already seeded");
                return;
            }

            _logger.LogInformation("Starting to seed 1000 products...");

            var categories = new[] { "Electronics", "Clothing", "Food", "Books", "Toys", "Sports", "Home", "Garden" };
            var random = new Random();
            var products = new List<Product>();

            for (int i = 1; i <= 1000; i++)
            {
                var product = new Product
                {
                    Name = $"Product {i}",
                    Description = $"Description for product {i}. This is a great product with excellent features.",
                    Price = Math.Round((decimal)(random.NextDouble() * 1000 + 10), 2),
                    StockQuantity = random.Next(0, 500),
                    Category = categories[random.Next(categories.Length)],
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                products.Add(product);

                // Batch insert every 100 products
                if (i % 100 == 0)
                {
                    _writeContext.Products.AddRange(products);
                    await _writeContext.SaveChangesAsync();

                    // Also add to read database
                    _readContext.Products.AddRange(products.Select(p => new Product
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        Price = p.Price,
                        StockQuantity = p.StockQuantity,
                        Category = p.Category,
                        CreatedAt = p.CreatedAt,
                        UpdatedAt = p.UpdatedAt
                    }));
                    await _readContext.SaveChangesAsync();

                    _logger.LogInformation("Seeded {Count} products", i);
                    products.Clear();
                }
            }

            _logger.LogInformation("Successfully seeded 1000 products");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding database");
            throw;
        }
    }
}
