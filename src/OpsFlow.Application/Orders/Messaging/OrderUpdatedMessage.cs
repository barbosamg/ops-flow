using OpsFlow.Domain.Orders;

namespace OpsFlow.Application.Orders.Messaging;

public sealed record OrderUpdatedMessage(
    Guid OrderId,
    OrderStatus Status,
    Guid AttemptId,
    IntegrationAttemptStatus AttemptStatus,
    string CorrelationId,
    DateTimeOffset OccurredAtUtc);
