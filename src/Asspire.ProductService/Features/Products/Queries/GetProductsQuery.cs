using Asspire.ProductService.Data;
using Asspire.ProductService.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Asspire.ProductService.Features.Products.Queries;

public record GetProductsQuery(int PageNumber = 1, int PageSize = 10) : IRequest<ProductsResult>;

public record ProductsResult(List<Product> Products, int TotalCount);

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, ProductsResult>
{
    private readonly ProductReadDbContext _readContext;

    public GetProductsQueryHandler(ProductReadDbContext readContext)
    {
        _readContext = readContext;
    }

    public async Task<ProductsResult> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var query = _readContext.Products.AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);

        var products = await query
            .OrderBy(p => p.Id)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new ProductsResult(products, totalCount);
    }
}
