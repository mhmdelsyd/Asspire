using Asspire.ProductService.Features.Products.Queries;
using Asspire.ProductService.Grpc;
using Grpc.Core;
using MediatR;

namespace Asspire.ProductService.Services;

public class ProductGrpcService : ProductGrpc.ProductGrpcBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProductGrpcService> _logger;

    public ProductGrpcService(IMediator mediator, ILogger<ProductGrpcService> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override async Task<ProductReply> GetProduct(GetProductRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Getting product with ID: {ProductId}", request.Id);

        var product = await _mediator.Send(new GetProductByIdQuery(request.Id));

        if (product == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Product with ID {request.Id} not found"));
        }

        return new ProductReply
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = (double)product.Price,
            StockQuantity = product.StockQuantity,
            Category = product.Category
        };
    }

    public override async Task<ProductsReply> GetProducts(GetProductsRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Getting products. Page: {PageNumber}, Size: {PageSize}",
            request.PageNumber, request.PageSize);

        var result = await _mediator.Send(new GetProductsQuery(request.PageNumber, request.PageSize));

        var reply = new ProductsReply
        {
            TotalCount = result.TotalCount
        };

        reply.Products.AddRange(result.Products.Select(p => new ProductReply
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = (double)p.Price,
            StockQuantity = p.StockQuantity,
            Category = p.Category
        }));

        return reply;
    }

    public override async Task<AvailabilityReply> CheckProductAvailability(
        CheckAvailabilityRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation("Checking availability for product {ProductId}, quantity: {Quantity}",
            request.ProductId, request.Quantity);

        var product = await _mediator.Send(new GetProductByIdQuery(request.ProductId));

        if (product == null)
        {
            return new AvailabilityReply
            {
                IsAvailable = false,
                AvailableQuantity = 0
            };
        }

        return new AvailabilityReply
        {
            IsAvailable = product.StockQuantity >= request.Quantity,
            AvailableQuantity = product.StockQuantity
        };
    }
}
