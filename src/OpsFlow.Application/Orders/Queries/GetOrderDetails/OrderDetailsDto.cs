using OpsFlow.Domain.Orders;

namespace OpsFlow.Application.Orders.Queries.GetOrderDetails;

public sealed record OrderDetailsDto(
    Guid Id,
    string Number,
    Guid CustomerId,
    string CustomerName,
    string CustomerEmail,
    Guid ProviderId,
    string ProviderName,
    decimal Amount,
    OrderStatus Status,
    string? Notes,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    string RowVersion,
    IReadOnlyList<OrderStatusHistoryDto> StatusHistory,
    IReadOnlyList<IntegrationAttemptDto> IntegrationAttempts);

public sealed record OrderStatusHistoryDto(
    Guid Id,
    OrderStatus PreviousStatus,
    OrderStatus NewStatus,
    string Reason,
    string ChangedBy,
    DateTimeOffset ChangedAtUtc);

public sealed record IntegrationAttemptDto(
    Guid Id,
    int AttemptNumber,
    string CorrelationId,
    IntegrationAttemptStatus Status,
    DateTimeOffset QueuedAtUtc,
    DateTimeOffset? StartedAtUtc,
    DateTimeOffset? FinishedAtUtc,
    int? DurationMilliseconds,
    string? ErrorCode,
    string? SanitizedError);
