namespace OpsFlow.Domain.Orders;

public enum IntegrationAttemptStatus
{
    Queued = 0,
    Processing = 1,
    Succeeded = 2,
    Failed = 3,
    TimedOut = 4
}
