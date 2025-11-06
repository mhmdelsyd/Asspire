using Asspire.OrderService.Data;
using Asspire.OrderService.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace Asspire.OrderService.IntegrationEvents;

public class OrderCreatedEventConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly OrderReadDbContext _readContext;
    private readonly IDistributedCache _cache;

    public OrderCreatedEventConsumer(OrderReadDbContext readContext, IDistributedCache cache)
    {
        _readContext = readContext;
        _cache = cache;
    }

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var order = new Order
        {
            Id = context.Message.Id,
            ProductId = context.Message.ProductId,
            ProductName = context.Message.ProductName,
            Quantity = context.Message.Quantity,
            UnitPrice = context.Message.UnitPrice,
            TotalPrice = context.Message.TotalPrice,
            Status = Enum.Parse<OrderStatus>(context.Message.Status),
            CustomerName = context.Message.CustomerName,
            CustomerEmail = context.Message.CustomerEmail,
            CreatedAt = context.Message.CreatedAt,
            UpdatedAt = context.Message.UpdatedAt
        };

        _readContext.Orders.Add(order);
        await _readContext.SaveChangesAsync();

        // Invalidate cache
        await _cache.RemoveAsync($"order:{order.Id}");
    }
}

public class OrderStatusUpdatedEventConsumer : IConsumer<OrderStatusUpdatedEvent>
{
    private readonly OrderReadDbContext _readContext;
    private readonly IDistributedCache _cache;

    public OrderStatusUpdatedEventConsumer(OrderReadDbContext readContext, IDistributedCache cache)
    {
        _readContext = readContext;
        _cache = cache;
    }

    public async Task Consume(ConsumeContext<OrderStatusUpdatedEvent> context)
    {
        var order = await _readContext.Orders
            .FirstOrDefaultAsync(o => o.Id == context.Message.Id);

        if (order != null)
        {
            order.Status = Enum.Parse<OrderStatus>(context.Message.Status);
            order.UpdatedAt = context.Message.UpdatedAt;

            await _readContext.SaveChangesAsync();

            // Invalidate cache
            await _cache.RemoveAsync($"order:{order.Id}");
        }
    }
}
