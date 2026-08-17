
using OpsFlow.Application.Common.Pagination;
using OpsFlow.Application.Orders.Ports;
using OpsFlow.Application.Orders.Queries.GetOrders;
using OpsFlow.Domain.Orders;

namespace OpsFlow.Infrastructure.Orders;

public sealed class InMemoryOrderReadRepository : IOrderReadRepository
{
    private static readonly IReadOnlyList<OrderListItemDto> Orders =
    [
        new(
            Guid.Parse("00000000-0000-0000-0000-000000000001"),
            "ORD-2026-0001",
            Guid.Parse("10000000-0000-0000-0000-000000000001"),
            "Northwind Market",
            "operations@northwind.example",
            Guid.Parse("20000000-0000-0000-0000-000000000001"),
            "Atlas Gateway",
            1_240.50m,
            new DateTimeOffset(2026, 8, 17, 12, 15, 0, TimeSpan.Zero),
            OrderStatus.Processing),

        new(
            Guid.Parse("00000000-0000-0000-0000-000000000002"),
            "ORD-2026-0002",
            Guid.Parse("10000000-0000-0000-0000-000000000002"),
            "Contoso Retail",
            "orders@contoso.example",
            Guid.Parse("20000000-0000-0000-0000-000000000002"),
            "Northstar ERP",
            489.90m,
            new DateTimeOffset(2026, 8, 17, 11, 40, 0, TimeSpan.Zero),
            OrderStatus.Completed),

        new(
            Guid.Parse("00000000-0000-0000-0000-000000000003"),
            "ORD-2026-0003",
            Guid.Parse("10000000-0000-0000-0000-000000000003"),
            "Fabrikam Stores",
            "support@fabrikam.example",
            Guid.Parse("20000000-0000-0000-0000-000000000003"),
            "Vertex Logistics",
            8_750.00m,
            new DateTimeOffset(2026, 8, 16, 20, 30, 0, TimeSpan.Zero),
            OrderStatus.Failed),

        new(
            Guid.Parse("00000000-0000-0000-0000-000000000004"),
            "ORD-2026-0004",
            Guid.Parse("10000000-0000-0000-0000-000000000004"),
            "Adventure Works",
            "sales@adventure-works.example",
            Guid.Parse("20000000-0000-0000-0000-000000000001"),
            "Atlas Gateway",
            219.99m,
            new DateTimeOffset(2026, 8, 16, 17, 10, 0, TimeSpan.Zero),
            OrderStatus.Pending),

        new(
            Guid.Parse("00000000-0000-0000-0000-000000000005"),
            "ORD-2026-0005",
            Guid.Parse("10000000-0000-0000-0000-000000000005"),
            "Tailspin Toys",
            "orders@tailspin.example",
            Guid.Parse("20000000-0000-0000-0000-000000000002"),
            "Northstar ERP",
            3_420.75m,
            new DateTimeOffset(2026, 8, 16, 14, 25, 0, TimeSpan.Zero),
            OrderStatus.Completed),

        new(
            Guid.Parse("00000000-0000-0000-0000-000000000006"),
            "ORD-2026-0006",
            Guid.Parse("10000000-0000-0000-0000-000000000006"),
            "Wide World Importers",
            "operations@wideworld.example",
            Guid.Parse("20000000-0000-0000-0000-000000000003"),
            "Vertex Logistics",
            12_999.00m,
            new DateTimeOffset(2026, 8, 15, 19, 45, 0, TimeSpan.Zero),
            OrderStatus.Processing),

        new(
            Guid.Parse("00000000-0000-0000-0000-000000000007"),
            "ORD-2026-0007",
            Guid.Parse("10000000-0000-0000-0000-000000000007"),
            "Blue Yonder Airlines",
            "procurement@blueyonder.example",
            Guid.Parse("20000000-0000-0000-0000-000000000001"),
            "Atlas Gateway",
            799.50m,
            new DateTimeOffset(2026, 8, 15, 16, 20, 0, TimeSpan.Zero),
            OrderStatus.Cancelled),

        new(
            Guid.Parse("00000000-0000-0000-0000-000000000008"),
            "ORD-2026-0008",
            Guid.Parse("10000000-0000-0000-0000-000000000008"),
            "Proseware Services",
            "billing@proseware.example",
            Guid.Parse("20000000-0000-0000-0000-000000000002"),
            "Northstar ERP",
            5_600.00m,
            new DateTimeOffset(2026, 8, 15, 13, 5, 0, TimeSpan.Zero),
            OrderStatus.Failed)
    ];

    public Task<PagedResult<OrderListItemDto>> SearchAsync(
        GetOrdersQuery query,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IEnumerable<OrderListItemDto> filteredOrders = Orders;

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();

            filteredOrders = filteredOrders.Where(order =>
                order.Number.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                order.CustomerName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                order.CustomerEmail.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                order.ProviderName.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        if (query.Status.HasValue)
        {
            filteredOrders = filteredOrders.Where(
                order => order.Status == query.Status.Value);
        }

        if (query.CustomerId.HasValue)
        {
            filteredOrders = filteredOrders.Where(
                order => order.CustomerId == query.CustomerId.Value);
        }

        if (query.ProviderId.HasValue)
        {
            filteredOrders = filteredOrders.Where(
                order => order.ProviderId == query.ProviderId.Value);
        }

        if (query.CreatedFromUtc.HasValue)
        {
            filteredOrders = filteredOrders.Where(
                order => order.CreatedAtUtc >= query.CreatedFromUtc.Value);
        }

        if (query.CreatedToUtc.HasValue)
        {
            filteredOrders = filteredOrders.Where(
                order => order.CreatedAtUtc <= query.CreatedToUtc.Value);
        }

        if (query.MinAmount.HasValue)
        {
            filteredOrders = filteredOrders.Where(
                order => order.Amount >= query.MinAmount.Value);
        }

        if (query.MaxAmount.HasValue)
        {
            filteredOrders = filteredOrders.Where(
                order => order.Amount <= query.MaxAmount.Value);
        }

        var totalCount = filteredOrders.Count();

        var orderedOrders = ApplySorting(filteredOrders, query.Sort);

        var skip = (long)(query.Page - 1) * query.PageSize;

        IReadOnlyList<OrderListItemDto> pageItems =
            skip >= totalCount
                ? []
                : orderedOrders
                    .Skip((int)skip)
                    .Take(query.PageSize)
                    .ToArray();

        var result = new PagedResult<OrderListItemDto>(
            pageItems,
            query.Page,
            query.PageSize,
            totalCount);

        return Task.FromResult(result);
    }

    private static IOrderedEnumerable<OrderListItemDto> ApplySorting(
        IEnumerable<OrderListItemDto> orders,
        string? sort)
    {
        return sort?.ToUpperInvariant() switch
        {
            "NUMBER" => orders.OrderBy(
                order => order.Number,
                StringComparer.OrdinalIgnoreCase),

            "-NUMBER" => orders.OrderByDescending(
                order => order.Number,
                StringComparer.OrdinalIgnoreCase),

            "CUSTOMERNAME" => orders.OrderBy(
                order => order.CustomerName,
                StringComparer.OrdinalIgnoreCase),

            "-CUSTOMERNAME" => orders.OrderByDescending(
                order => order.CustomerName,
                StringComparer.OrdinalIgnoreCase),

            "PROVIDERNAME" => orders.OrderBy(
                order => order.ProviderName,
                StringComparer.OrdinalIgnoreCase),

            "-PROVIDERNAME" => orders.OrderByDescending(
                order => order.ProviderName,
                StringComparer.OrdinalIgnoreCase),

            "AMOUNT" => orders.OrderBy(order => order.Amount),

            "-AMOUNT" => orders.OrderByDescending(order => order.Amount),

            "CREATEDATUTC" => orders.OrderBy(order => order.CreatedAtUtc),

            "-CREATEDATUTC" => orders.OrderByDescending(
                order => order.CreatedAtUtc),

            "STATUS" => orders.OrderBy(order => order.Status),

            "-STATUS" => orders.OrderByDescending(order => order.Status),

            _ => orders.OrderByDescending(order => order.CreatedAtUtc)
        };
    }
}
