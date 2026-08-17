namespace OpsFlow.Web.Models.Orders;

public sealed record OrderRetryAccepted(
    Guid OrderId,
    Guid AttemptId,
    int AttemptNumber,
    string CorrelationId,
    string Status);
