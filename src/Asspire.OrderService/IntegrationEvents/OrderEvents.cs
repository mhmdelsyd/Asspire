namespace Asspire.OrderService.IntegrationEvents;

public class OrderCreatedEvent
{
    public Guid Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class OrderStatusUpdatedEvent
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}
