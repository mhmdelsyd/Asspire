using Asspire.OrderService.Data;
using Asspire.OrderService.IntegrationEvents;
using Asspire.OrderService.Models;
using MassTransit;
using MediatR;
using System.Text.Json;

namespace Asspire.OrderService.Features.Orders.Commands;

public record CreateOrderCommand(
    int ProductId,
    int Quantity,
    string CustomerName,
    string CustomerEmail
) : IRequest<Order?>;

// DTOs for Product Service HTTP responses
public record ProductDto(int Id, string Name, string Description, decimal Price, int StockQuantity, string Category);

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Order?>
{
    private readonly OrderWriteDbContext _writeContext;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<CreateOrderCommandHandler> _logger;
    private readonly IConfiguration _configuration;

    public CreateOrderCommandHandler(
        OrderWriteDbContext writeContext,
        IHttpClientFactory httpClientFactory,
        IPublishEndpoint publishEndpoint,
        ILogger<CreateOrderCommandHandler> logger,
        IConfiguration configuration)
    {
        _writeContext = writeContext;
        _httpClientFactory = httpClientFactory;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<Order?> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get product details via HTTP
            var httpClient = _httpClientFactory.CreateClient();
            var productServiceUrl = _configuration["SERVICES__PRODUCTSERVICE__HTTP__0"] ?? "http://product-service:8080";

            _logger.LogInformation("Fetching product {ProductId} from {Url}", request.ProductId, productServiceUrl);

            var productResponse = await httpClient.GetAsync(
                $"{productServiceUrl}/api/products/{request.ProductId}",
                cancellationToken);

            if (!productResponse.IsSuccessStatusCode)
            {
                _logger.LogWarning("Product {ProductId} not found. Status: {StatusCode}",
                    request.ProductId, productResponse.StatusCode);
                return null;
            }

            var productJson = await productResponse.Content.ReadAsStringAsync(cancellationToken);
            var product = JsonSerializer.Deserialize<ProductDto>(productJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (product == null)
            {
                _logger.LogWarning("Product {ProductId} could not be deserialized", request.ProductId);
                return null;
            }

            // Check availability
            if (product.StockQuantity < request.Quantity)
            {
                _logger.LogWarning("Product {ProductId} insufficient stock. Requested: {Requested}, Available: {Available}",
                    request.ProductId, request.Quantity, product.StockQuantity);
                return null;
            }

            // Create order with product details
            var order = new Order
            {
                Id = Guid.NewGuid(),
                ProductId = request.ProductId,
                ProductName = product.Name,
                Quantity = request.Quantity,
                UnitPrice = product.Price,
                TotalPrice = product.Price * request.Quantity,
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
