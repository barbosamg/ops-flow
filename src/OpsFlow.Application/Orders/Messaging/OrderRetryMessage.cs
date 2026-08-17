namespace OpsFlow.Application.Orders.Messaging;

public sealed record OrderRetryMessage(
    Guid OrderId,
    Guid AttemptId,
    string CorrelationId);
