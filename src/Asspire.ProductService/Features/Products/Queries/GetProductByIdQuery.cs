using Asspire.ProductService.Data;
using Asspire.ProductService.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Asspire.ProductService.Features.Products.Queries;

public record GetProductByIdQuery(int Id) : IRequest<Product?>;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, Product?>
{
    private readonly ProductReadDbContext _readContext;
    private readonly IDistributedCache _cache;

    public GetProductByIdQueryHandler(
        ProductReadDbContext readContext,
        IDistributedCache cache)
    {
        _readContext = readContext;
        _cache = cache;
    }

    public async Task<Product?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        // Try to get from cache first
        var cacheKey = $"product:{request.Id}";
        var cachedProduct = await _cache.GetStringAsync(cacheKey, cancellationToken);

        if (!string.IsNullOrEmpty(cachedProduct))
        {
            return JsonSerializer.Deserialize<Product>(cachedProduct);
        }

        // If not in cache, get from database
        var product = await _readContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        // Cache the result
        if (product != null)
        {
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };
            await _cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(product),
                cacheOptions,
                cancellationToken);
        }

        return product;
    }
}
