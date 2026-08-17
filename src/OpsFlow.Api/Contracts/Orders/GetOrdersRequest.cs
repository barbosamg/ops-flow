
using OpsFlow.Application.Orders.Queries.GetOrders;
using OpsFlow.Domain.Orders;

namespace OpsFlow.Api.Contracts.Orders;

public sealed class GetOrdersRequest
{
    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 25;

    public string? Search { get; init; }

    public OrderStatus? Status { get; init; }

    public Guid? CustomerId { get; init; }

    public Guid? ProviderId { get; init; }

    public DateTimeOffset? CreatedFromUtc { get; init; }

    public DateTimeOffset? CreatedToUtc { get; init; }

    public decimal? MinAmount { get; init; }

    public decimal? MaxAmount { get; init; }

    public string? Sort { get; init; }

    public GetOrdersQuery ToQuery()
    {
        return new GetOrdersQuery(
            Page,
            PageSize,
            Search,
            Status,
            CustomerId,
            ProviderId,
            CreatedFromUtc,
            CreatedToUtc,
            MinAmount,
            MaxAmount,
            Sort);
    }
}