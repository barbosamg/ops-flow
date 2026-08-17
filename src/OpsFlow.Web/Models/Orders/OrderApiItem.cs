
namespace OpsFlow.Web.Models.Orders;

public sealed record OrderApiItem(
    Guid Id,
    string Number,
    Guid CustomerId,
    string CustomerName,
    string CustomerEmail,
    Guid ProviderId,
    string ProviderName,
    decimal Amount,
    DateTimeOffset CreatedAtUtc,
    string Status);