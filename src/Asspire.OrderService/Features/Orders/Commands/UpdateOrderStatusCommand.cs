using Asspire.OrderService.Data;
using Asspire.OrderService.IntegrationEvents;
using Asspire.OrderService.Models;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Asspire.OrderService.Features.Orders.Commands;

public record UpdateOrderStatusCommand(Guid Id, OrderStatus Status) : IRequest<Order?>;

public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, Order?>
{
    private readonly OrderWriteDbContext _writeContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public UpdateOrderStatusCommandHandler(
        OrderWriteDbContext writeContext,
        IPublishEndpoint publishEndpoint)
    {
        _writeContext = writeContext;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Order?> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _writeContext.Orders
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        if (order == null)
            return null;

        order.Status = request.Status;
        order.UpdatedAt = DateTime.UtcNow;

        await _writeContext.SaveChangesAsync(cancellationToken);

        // Publish integration event
        await _publishEndpoint.Publish(new OrderStatusUpdatedEvent
        {
            Id = order.Id,
            Status = order.Status.ToString(),
            UpdatedAt = order.UpdatedAt
        }, cancellationToken);

        return order;
    }
}
