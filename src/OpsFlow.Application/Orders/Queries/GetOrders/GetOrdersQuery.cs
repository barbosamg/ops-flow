using OpsFlow.Domain.Orders;

namespace OpsFlow.Application.Orders.Queries.GetOrders;

public sealed record GetOrdersQuery(
    int Page = 1,
    int PageSize = 25,
    string? Search = null,
    OrderStatus? Status = null,
    Guid? CustomerId = null,
    Guid? ProviderId = null,
    DateTimeOffset? CreatedFromUtc = null,
    DateTimeOffset? CreatedToUtc = null,
    decimal? MinAmount = null,
    decimal? MaxAmount = null,
    string? Sort = null);