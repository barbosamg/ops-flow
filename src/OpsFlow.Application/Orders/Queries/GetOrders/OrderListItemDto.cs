
using OpsFlow.Domain.Orders;

namespace OpsFlow.Application.Orders.Queries.GetOrders;

public sealed record OrderListItemDto(
    Guid Id,
    string Number,
    Guid CustomerId,
    string CustomerName,
    string CustomerEmail,
    Guid ProviderId,
    string ProviderName,
    decimal Amount,
    DateTimeOffset CreatedAtUtc,
    OrderStatus Status);