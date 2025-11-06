using Asspire.ProductService.Data;
using Asspire.ProductService.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace Asspire.ProductService.IntegrationEvents;

public class ProductCreatedEventConsumer : IConsumer<ProductCreatedEvent>
{
    private readonly ProductReadDbContext _readContext;
    private readonly IDistributedCache _cache;

    public ProductCreatedEventConsumer(ProductReadDbContext readContext, IDistributedCache cache)
    {
        _readContext = readContext;
        _cache = cache;
    }

    public async Task Consume(ConsumeContext<ProductCreatedEvent> context)
    {
        var product = new Product
        {
            Id = context.Message.Id,
            Name = context.Message.Name,
            Description = context.Message.Description,
            Price = context.Message.Price,
            StockQuantity = context.Message.StockQuantity,
            Category = context.Message.Category,
            CreatedAt = context.Message.CreatedAt,
            UpdatedAt = context.Message.UpdatedAt
        };

        _readContext.Products.Add(product);
        await _readContext.SaveChangesAsync();

        // Invalidate cache
        await _cache.RemoveAsync($"product:{product.Id}");
    }
}

public class ProductUpdatedEventConsumer : IConsumer<ProductUpdatedEvent>
{
    private readonly ProductReadDbContext _readContext;
    private readonly IDistributedCache _cache;

    public ProductUpdatedEventConsumer(ProductReadDbContext readContext, IDistributedCache cache)
    {
        _readContext = readContext;
        _cache = cache;
    }

    public async Task Consume(ConsumeContext<ProductUpdatedEvent> context)
    {
        var product = await _readContext.Products
            .FirstOrDefaultAsync(p => p.Id == context.Message.Id);

        if (product != null)
        {
            product.Name = context.Message.Name;
            product.Description = context.Message.Description;
            product.Price = context.Message.Price;
            product.StockQuantity = context.Message.StockQuantity;
            product.Category = context.Message.Category;
            product.UpdatedAt = context.Message.UpdatedAt;

            await _readContext.SaveChangesAsync();

            // Invalidate cache
            await _cache.RemoveAsync($"product:{product.Id}");
        }
    }
}

public class ProductDeletedEventConsumer : IConsumer<ProductDeletedEvent>
{
    private readonly ProductReadDbContext _readContext;
    private readonly IDistributedCache _cache;

    public ProductDeletedEventConsumer(ProductReadDbContext readContext, IDistributedCache cache)
    {
        _readContext = readContext;
        _cache = cache;
    }

    public async Task Consume(ConsumeContext<ProductDeletedEvent> context)
    {
        var product = await _readContext.Products
            .FirstOrDefaultAsync(p => p.Id == context.Message.Id);

        if (product != null)
        {
            _readContext.Products.Remove(product);
            await _readContext.SaveChangesAsync();

            // Invalidate cache
            await _cache.RemoveAsync($"product:{product.Id}");
        }
    }
}
