namespace OpsFlow.Web.Models.Orders;

public sealed record OrderListItem(
    Guid Id,
    string Number,
    string CustomerName,
    string ProviderName,
    decimal Total,
    DateTimeOffset CreatedAt,
    string Status);