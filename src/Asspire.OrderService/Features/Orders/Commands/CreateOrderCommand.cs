using Asspire.OrderService.Data;
using Asspire.OrderService.Grpc;
using Asspire.OrderService.IntegrationEvents;
using Asspire.OrderService.Models;
using MassTransit;
using MediatR;

namespace Asspire.OrderService.Features.Orders.Commands;

public record CreateOrderCommand(
    int ProductId,
    int Quantity,
    string CustomerName,
    string CustomerEmail
) : IRequest<Order?>;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Order?>
{
    private readonly OrderWriteDbContext _writeContext;
    private readonly ProductGrpc.ProductGrpcClient _productClient;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<CreateOrderCommandHandler> _logger;

    public CreateOrderCommandHandler(
        OrderWriteDbContext writeContext,
        ProductGrpc.ProductGrpcClient productClient,
        IPublishEndpoint publishEndpoint,
        ILogger<CreateOrderCommandHandler> logger)
    {
        _writeContext = writeContext;
        _productClient = productClient;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<Order?> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check product availability via gRPC
            var availabilityResponse = await _productClient.CheckProductAvailabilityAsync(
                new CheckAvailabilityRequest
                {
                    ProductId = request.ProductId,
                    Quantity = request.Quantity
                }, cancellationToken: cancellationToken);

            if (!availabilityResponse.IsAvailable)
            {
                _logger.LogWarning("Product {ProductId} not available. Requested: {Requested}, Available: {Available}",
                    request.ProductId, request.Quantity, availabilityResponse.AvailableQuantity);
                return null;
            }

            // Get product details via gRPC
            var productResponse = await _productClient.GetProductAsync(
                new GetProductRequest { Id = request.ProductId },
                cancellationToken: cancellationToken);

            var order = new Order
            {
                Id = Guid.NewGuid(),
                ProductId = request.ProductId,
                ProductName = productResponse.Name,
                Quantity = request.Quantity,
                UnitPrice = (decimal)productResponse.Price,
                TotalPrice = (decimal)productResponse.Price * request.Quantity,
                Status = OrderStatus.Pending,
                CustomerName = request.CustomerName,
                CustomerEmail = request.CustomerEmail,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _writeContext.Orders.Add(order);
            await _writeContext.SaveChangesAsync(cancellationToken);

            // Publish integration event
            await _publishEndpoint.Publish(new OrderCreatedEvent
            {
                Id = order.Id,
                ProductId = order.ProductId,
                ProductName = order.ProductName,
                Quantity = order.Quantity,
                UnitPrice = order.UnitPrice,
                TotalPrice = order.TotalPrice,
                Status = order.Status.ToString(),
                CustomerName = order.CustomerName,
                CustomerEmail = order.CustomerEmail,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt
            }, cancellationToken);

            _logger.LogInformation("Order {OrderId} created successfully", order.Id);

            return order;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order for product {ProductId}", request.ProductId);
            throw;
        }
    }
}
