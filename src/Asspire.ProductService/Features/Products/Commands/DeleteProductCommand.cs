using Asspire.ProductService.Data;
using Asspire.ProductService.IntegrationEvents;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Asspire.ProductService.Features.Products.Commands;

public record DeleteProductCommand(int Id) : IRequest<bool>;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, bool>
{
    private readonly ProductWriteDbContext _writeContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public DeleteProductCommandHandler(
        ProductWriteDbContext writeContext,
        IPublishEndpoint publishEndpoint)
    {
        _writeContext = writeContext;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _writeContext.Products
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (product == null)
            return false;

        _writeContext.Products.Remove(product);
        await _writeContext.SaveChangesAsync(cancellationToken);

        // Publish integration event to sync read database
        await _publishEndpoint.Publish(new ProductDeletedEvent
        {
            Id = product.Id
        }, cancellationToken);

        return true;
    }
}
