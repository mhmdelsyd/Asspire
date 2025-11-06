namespace Asspire.ProductService.IntegrationEvents;

public class ProductCreatedEvent
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string Category { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ProductUpdatedEvent
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string Category { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}

public class ProductDeletedEvent
{
    public int Id { get; set; }
}

public class ProductStockReservedEvent
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public Guid OrderId { get; set; }
}
