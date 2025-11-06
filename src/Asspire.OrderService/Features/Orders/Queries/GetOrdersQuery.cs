using Asspire.OrderService.Data;
using Asspire.OrderService.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Asspire.OrderService.Features.Orders.Queries;

public record GetOrdersQuery(int PageNumber = 1, int PageSize = 10) : IRequest<OrdersResult>;

public record OrdersResult(List<Order> Orders, int TotalCount);

public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, OrdersResult>
{
    private readonly OrderReadDbContext _readContext;

    public GetOrdersQueryHandler(OrderReadDbContext readContext)
    {
        _readContext = readContext;
    }

    public async Task<OrdersResult> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = _readContext.Orders.AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new OrdersResult(orders, totalCount);
    }
}
