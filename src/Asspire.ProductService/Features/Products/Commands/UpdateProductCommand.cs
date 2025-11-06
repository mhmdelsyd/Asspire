using Asspire.ProductService.Data;
using Asspire.ProductService.IntegrationEvents;
using Asspire.ProductService.Models;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Asspire.ProductService.Features.Products.Commands;

public record UpdateProductCommand(
    int Id,
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    string Category
) : IRequest<Product?>;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Product?>
{
    private readonly ProductWriteDbContext _writeContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public UpdateProductCommandHandler(
        ProductWriteDbContext writeContext,
        IPublishEndpoint publishEndpoint)
    {
        _writeContext = writeContext;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Product?> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _writeContext.Products
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (product == null)
            return null;

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.StockQuantity = request.StockQuantity;
        product.Category = request.Category;
        product.UpdatedAt = DateTime.UtcNow;

        await _writeContext.SaveChangesAsync(cancellationToken);

        // Publish integration event to sync read database
        await _publishEndpoint.Publish(new ProductUpdatedEvent
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            Category = product.Category,
            UpdatedAt = product.UpdatedAt
        }, cancellationToken);

        return product;
    }
}
