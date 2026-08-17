namespace OpsFlow.Web.Models.Orders;

public sealed record OrderDetails(
    Guid Id,
    string Number,
    Guid CustomerId,
    string CustomerName,
    string CustomerEmail,
    Guid ProviderId,
    string ProviderName,
    decimal Amount,
    string Status,
    string? Notes,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    string RowVersion,
    IReadOnlyList<OrderStatusHistoryItem> StatusHistory,
    IReadOnlyList<IntegrationAttemptItem> IntegrationAttempts);

public sealed record OrderStatusHistoryItem(
    Guid Id,
    string PreviousStatus,
    string NewStatus,
    string Reason,
    string ChangedBy,
    DateTimeOffset ChangedAtUtc);

public sealed record IntegrationAttemptItem(
    Guid Id,
    int AttemptNumber,
    string CorrelationId,
    string Status,
    DateTimeOffset QueuedAtUtc,
    DateTimeOffset? StartedAtUtc,
    DateTimeOffset? FinishedAtUtc,
    int? DurationMilliseconds,
    string? ErrorCode,
    string? SanitizedError);
