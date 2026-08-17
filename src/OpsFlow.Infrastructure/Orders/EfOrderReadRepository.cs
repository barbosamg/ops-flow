using Microsoft.EntityFrameworkCore;
using OpsFlow.Application.Common.Pagination;
using OpsFlow.Application.Orders.Ports;
using OpsFlow.Application.Orders.Queries.GetOrders;
using OpsFlow.Domain.Orders;
using OpsFlow.Infrastructure.Persistence;

namespace OpsFlow.Infrastructure.Orders;

public sealed class EfOrderReadRepository(OpsFlowDbContext dbContext) :
    IOrderReadRepository
{
    public async Task<PagedResult<OrderListItemDto>> SearchAsync(
        GetOrdersQuery query,
        CancellationToken cancellationToken)
    {
        var orders =
            from order in dbContext.Orders.AsNoTracking()
            join customer in dbContext.Customers.AsNoTracking()
                on order.CustomerId equals customer.Id
            join provider in dbContext.Providers.AsNoTracking()
                on order.ProviderId equals provider.Id
            select new OrderReadRow
            {
                Id = order.Id,
                Number = order.Number,
                CustomerId = customer.Id,
                CustomerName = customer.Name,
                CustomerEmail = customer.Email,
                ProviderId = provider.Id,
                ProviderName = provider.Name,
                Amount = order.Amount,
                CreatedAtUtc = order.CreatedAtUtc,
                Status = order.Status
            };

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            orders = orders.Where(order =>
                order.Number.Contains(search) ||
                order.CustomerName.Contains(search) ||
                order.CustomerEmail.Contains(search) ||
                order.ProviderName.Contains(search));
        }

        if (query.Status.HasValue)
        {
            orders = orders.Where(order => order.Status == query.Status.Value);
        }

        if (query.CustomerId.HasValue)
        {
            orders = orders.Where(order =>
                order.CustomerId == query.CustomerId.Value);
        }

        if (query.ProviderId.HasValue)
        {
            orders = orders.Where(order =>
                order.ProviderId == query.ProviderId.Value);
        }

        if (query.CreatedFromUtc.HasValue)
        {
            orders = orders.Where(order =>
                order.CreatedAtUtc >= query.CreatedFromUtc.Value);
        }

        if (query.CreatedToUtc.HasValue)
        {
            orders = orders.Where(order =>
                order.CreatedAtUtc <= query.CreatedToUtc.Value);
        }

        if (query.MinAmount.HasValue)
        {
            orders = orders.Where(order =>
                order.Amount >= query.MinAmount.Value);
        }

        if (query.MaxAmount.HasValue)
        {
            orders = orders.Where(order =>
                order.Amount <= query.MaxAmount.Value);
        }

        var totalCount = await orders.CountAsync(cancellationToken);
        var orderedOrders = ApplySorting(orders, query.Sort);
        var skip = (long)(query.Page - 1) * query.PageSize;

        var rows = skip >= totalCount
            ? []
            : await orderedOrders
                .Skip((int)skip)
                .Take(query.PageSize)
                .ToArrayAsync(cancellationToken);

        var items = rows
            .Select(order => new OrderListItemDto(
                order.Id,
                order.Number,
                order.CustomerId,
                order.CustomerName,
                order.CustomerEmail,
                order.ProviderId,
                order.ProviderName,
                order.Amount,
                order.CreatedAtUtc,
                order.Status))
            .ToArray();

        return new PagedResult<OrderListItemDto>(
            items,
            query.Page,
            query.PageSize,
            totalCount);
    }

    private static IOrderedQueryable<OrderReadRow> ApplySorting(
        IQueryable<OrderReadRow> orders,
        string? sort) =>
        sort?.ToUpperInvariant() switch
        {
            "NUMBER" => orders.OrderBy(order => order.Number),
            "-NUMBER" => orders.OrderByDescending(order => order.Number),
            "CUSTOMERNAME" => orders.OrderBy(order => order.CustomerName),
            "-CUSTOMERNAME" => orders.OrderByDescending(
                order => order.CustomerName),
            "PROVIDERNAME" => orders.OrderBy(order => order.ProviderName),
            "-PROVIDERNAME" => orders.OrderByDescending(
                order => order.ProviderName),
            "AMOUNT" => orders.OrderBy(order => order.Amount),
            "-AMOUNT" => orders.OrderByDescending(order => order.Amount),
            "CREATEDATUTC" => orders.OrderBy(order => order.CreatedAtUtc),
            "-CREATEDATUTC" => orders.OrderByDescending(
                order => order.CreatedAtUtc),
            "STATUS" => orders.OrderBy(order => order.Status),
            "-STATUS" => orders.OrderByDescending(order => order.Status),
            _ => orders.OrderByDescending(order => order.CreatedAtUtc)
        };

    private sealed class OrderReadRow
    {
        public Guid Id { get; init; }

        public string Number { get; init; } = string.Empty;

        public Guid CustomerId { get; init; }

        public string CustomerName { get; init; } = string.Empty;

        public string CustomerEmail { get; init; } = string.Empty;

        public Guid ProviderId { get; init; }

        public string ProviderName { get; init; } = string.Empty;

        public decimal Amount { get; init; }

        public DateTimeOffset CreatedAtUtc { get; init; }

        public OrderStatus Status { get; init; }
    }
}
