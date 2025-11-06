using Asspire.OrderService.Data;
using Asspire.OrderService.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Asspire.OrderService.Features.Orders.Queries;

public record GetOrderByIdQuery(Guid Id) : IRequest<Order?>;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, Order?>
{
    private readonly OrderReadDbContext _readContext;
    private readonly IDistributedCache _cache;

    public GetOrderByIdQueryHandler(
        OrderReadDbContext readContext,
        IDistributedCache cache)
    {
        _readContext = readContext;
        _cache = cache;
    }

    public async Task<Order?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        // Try to get from cache first
        var cacheKey = $"order:{request.Id}";
        var cachedOrder = await _cache.GetStringAsync(cacheKey, cancellationToken);

        if (!string.IsNullOrEmpty(cachedOrder))
        {
            return JsonSerializer.Deserialize<Order>(cachedOrder);
        }

        // If not in cache, get from database
        var order = await _readContext.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        // Cache the result
        if (order != null)
        {
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };
            await _cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(order),
                cacheOptions,
                cancellationToken);
        }

        return order;
    }
}
