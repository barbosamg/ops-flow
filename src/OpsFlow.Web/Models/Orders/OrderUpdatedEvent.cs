namespace OpsFlow.Web.Models.Orders;

public sealed record OrderUpdatedEvent(
    Guid OrderId,
    OrderStatusValue Status,
    Guid AttemptId,
    IntegrationAttemptStatusValue AttemptStatus,
    string CorrelationId,
    DateTimeOffset OccurredAtUtc);

// Os valores acompanham o contrato numérico padrão do SignalR.
public enum OrderStatusValue
{
    Draft = 0,
    Pending = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4,
    Cancelled = 5
}

public enum IntegrationAttemptStatusValue
{
    Queued = 0,
    Processing = 1,
    Succeeded = 2,
    Failed = 3,
    TimedOut = 4
}
