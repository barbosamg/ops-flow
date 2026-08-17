using OpsFlow.Domain.Common;

namespace OpsFlow.Domain.Orders;

public sealed class IntegrationAttempt
{
    private IntegrationAttempt()
    {
    }

    private IntegrationAttempt(
        Guid id,
        Guid orderId,
        int attemptNumber,
        string correlationId,
        DateTimeOffset queuedAtUtc)
    {
        Id = id;
        OrderId = orderId;
        AttemptNumber = attemptNumber;
        CorrelationId = correlationId;
        Status = IntegrationAttemptStatus.Queued;
        QueuedAtUtc = queuedAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid OrderId { get; private set; }

    public int AttemptNumber { get; private set; }

    public string CorrelationId { get; private set; } = string.Empty;

    public IntegrationAttemptStatus Status { get; private set; }

    public DateTimeOffset QueuedAtUtc { get; private set; }

    public DateTimeOffset? StartedAtUtc { get; private set; }

    public DateTimeOffset? FinishedAtUtc { get; private set; }

    public int? DurationMilliseconds { get; private set; }

    public string? ErrorCode { get; private set; }

    public string? SanitizedError { get; private set; }

    public bool IsActive =>
        Status is IntegrationAttemptStatus.Queued
            or IntegrationAttemptStatus.Processing;

    internal static IntegrationAttempt Queue(
        Guid orderId,
        int attemptNumber,
        string correlationId,
        DateTimeOffset queuedAtUtc)
    {
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            throw new DomainRuleException("Correlation id is required.");
        }

        return new IntegrationAttempt(
            Guid.NewGuid(),
            orderId,
            attemptNumber,
            correlationId.Trim(),
            queuedAtUtc);
    }

    internal void Start(DateTimeOffset startedAtUtc)
    {
        EnsureStatus(IntegrationAttemptStatus.Queued);
        Status = IntegrationAttemptStatus.Processing;
        StartedAtUtc = startedAtUtc;
    }

    internal void Succeed(DateTimeOffset finishedAtUtc)
    {
        Finish(IntegrationAttemptStatus.Succeeded, finishedAtUtc, null, null);
    }

    internal void Fail(
        DateTimeOffset finishedAtUtc,
        string errorCode,
        string sanitizedError)
    {
        Finish(
            IntegrationAttemptStatus.Failed,
            finishedAtUtc,
            errorCode,
            sanitizedError);
    }

    internal void TimeOut(DateTimeOffset finishedAtUtc)
    {
        Finish(
            IntegrationAttemptStatus.TimedOut,
            finishedAtUtc,
            "PROVIDER_TIMEOUT",
            "The provider did not respond within the configured timeout.");
    }

    private void Finish(
        IntegrationAttemptStatus status,
        DateTimeOffset finishedAtUtc,
        string? errorCode,
        string? sanitizedError)
    {
        EnsureStatus(IntegrationAttemptStatus.Processing);
        Status = status;
        FinishedAtUtc = finishedAtUtc;
        DurationMilliseconds = StartedAtUtc.HasValue
            ? Math.Max(0, (int)(finishedAtUtc - StartedAtUtc.Value).TotalMilliseconds)
            : null;
        ErrorCode = errorCode;
        SanitizedError = sanitizedError;
    }

    private void EnsureStatus(IntegrationAttemptStatus expected)
    {
        if (Status != expected)
        {
            throw new DomainRuleException(
                $"Attempt {AttemptNumber} must be {expected} to perform this operation.");
        }
    }
}
