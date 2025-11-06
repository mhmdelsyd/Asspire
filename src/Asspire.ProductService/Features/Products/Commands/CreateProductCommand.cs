using Asspire.ProductService.Data;
using Asspire.ProductService.IntegrationEvents;
using Asspire.ProductService.Models;
using MassTransit;
using MediatR;

namespace Asspire.ProductService.Features.Products.Commands;

public record CreateProductCommand(
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    string Category
) : IRequest<Product>;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Product>
{
    private readonly ProductWriteDbContext _writeContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreateProductCommandHandler(
        ProductWriteDbContext writeContext,
        IPublishEndpoint publishEndpoint)
    {
        _writeContext = writeContext;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Product> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            Category = request.Category,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _writeContext.Products.Add(product);
        await _writeContext.SaveChangesAsync(cancellationToken);

        // Publish integration event to sync read database
        await _publishEndpoint.Publish(new ProductCreatedEvent
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            Category = product.Category,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        }, cancellationToken);

        return product;
    }
}
