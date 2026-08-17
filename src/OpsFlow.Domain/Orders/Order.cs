using OpsFlow.Domain.Common;

namespace OpsFlow.Domain.Orders;

public sealed class Order
{
    private readonly List<IntegrationAttempt> _integrationAttempts = [];
    private readonly List<OrderStatusHistory> _statusHistory = [];

    private Order()
    {
    }

    private Order(
        Guid id,
        string number,
        Guid customerId,
        Guid providerId,
        decimal amount,
        string? notes,
        DateTimeOffset createdAtUtc)
    {
        Id = id;
        Number = number;
        CustomerId = customerId;
        ProviderId = providerId;
        Amount = amount;
        Notes = notes;
        Status = OrderStatus.Draft;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }

    public string Number { get; private set; } = string.Empty;

    public Guid CustomerId { get; private set; }

    public Guid ProviderId { get; private set; }

    public decimal Amount { get; private set; }

    public OrderStatus Status { get; private set; }

    public string? Notes { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public byte[] RowVersion { get; private set; } = [];

    public IReadOnlyCollection<OrderStatusHistory> StatusHistory => _statusHistory;

    public IReadOnlyCollection<IntegrationAttempt> IntegrationAttempts =>
        _integrationAttempts;

    public static Order Create(
        Guid id,
        string number,
        Guid customerId,
        Guid providerId,
        decimal amount,
        string? notes,
        DateTimeOffset createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new DomainRuleException("Order id is required.");
        }

        if (customerId == Guid.Empty || providerId == Guid.Empty)
        {
            throw new DomainRuleException("Customer and provider are required.");
        }

        return new Order(
            id,
            NormalizeNumber(number),
            customerId,
            providerId,
            PositiveAmount(amount),
            NormalizeNotes(notes),
            createdAtUtc);
    }

    public void Update(
        Guid customerId,
        Guid providerId,
        decimal amount,
        string? notes,
        DateTimeOffset changedAtUtc)
    {
        if (Status is not (OrderStatus.Draft or OrderStatus.Pending))
        {
            throw new DomainRuleException(
                $"Orders in {Status} status cannot be edited.");
        }

        if (customerId == Guid.Empty || providerId == Guid.Empty)
        {
            throw new DomainRuleException("Customer and provider are required.");
        }

        CustomerId = customerId;
        ProviderId = providerId;
        Amount = PositiveAmount(amount);
        Notes = NormalizeNotes(notes);
        UpdatedAtUtc = changedAtUtc;
    }

    public void Submit(string changedBy, DateTimeOffset changedAtUtc) =>
        TransitionTo(OrderStatus.Pending, "Order submitted.", changedBy, changedAtUtc);

    public void Cancel(
        string reason,
        string changedBy,
        DateTimeOffset changedAtUtc) =>
        TransitionTo(OrderStatus.Cancelled, reason, changedBy, changedAtUtc);

    public void StartProcessing(
        string changedBy,
        DateTimeOffset changedAtUtc) =>
        TransitionTo(
            OrderStatus.Processing,
            "Provider processing started.",
            changedBy,
            changedAtUtc);

    public void Complete(string changedBy, DateTimeOffset changedAtUtc) =>
        TransitionTo(
            OrderStatus.Completed,
            "Provider processing completed.",
            changedBy,
            changedAtUtc);

    public void MarkFailed(
        string reason,
        string changedBy,
        DateTimeOffset changedAtUtc) =>
        TransitionTo(OrderStatus.Failed, reason, changedBy, changedAtUtc);

    public IntegrationAttempt QueueRetry(
        string correlationId,
        DateTimeOffset queuedAtUtc)
    {
        var existingAttempt = _integrationAttempts.FirstOrDefault(
            attempt => string.Equals(
                attempt.CorrelationId,
                correlationId,
                StringComparison.Ordinal));

        if (existingAttempt is not null)
        {
            return existingAttempt;
        }

        if (Status != OrderStatus.Failed)
        {
            throw new DomainRuleException(
                "Only failed orders are eligible for retry.");
        }

        if (_integrationAttempts.Any(attempt => attempt.IsActive))
        {
            throw new DomainRuleException(
                "The order already has an active integration attempt.");
        }

        var nextAttemptNumber = _integrationAttempts.Count is 0
            ? 1
            : _integrationAttempts.Max(attempt => attempt.AttemptNumber) + 1;

        var attempt = IntegrationAttempt.Queue(
            Id,
            nextAttemptNumber,
            correlationId,
            queuedAtUtc);

        _integrationAttempts.Add(attempt);
        UpdatedAtUtc = queuedAtUtc;

        return attempt;
    }

    public void StartAttempt(
        Guid attemptId,
        string changedBy,
        DateTimeOffset startedAtUtc)
    {
        var attempt = FindAttempt(attemptId);
        attempt.Start(startedAtUtc);
        StartProcessing(changedBy, startedAtUtc);
    }

    public void CompleteAttempt(
        Guid attemptId,
        string changedBy,
        DateTimeOffset finishedAtUtc)
    {
        var attempt = FindAttempt(attemptId);
        attempt.Succeed(finishedAtUtc);
        Complete(changedBy, finishedAtUtc);
    }

    public void FailAttempt(
        Guid attemptId,
        string errorCode,
        string sanitizedError,
        string changedBy,
        DateTimeOffset finishedAtUtc)
    {
        var attempt = FindAttempt(attemptId);
        attempt.Fail(finishedAtUtc, errorCode, sanitizedError);
        MarkFailed(sanitizedError, changedBy, finishedAtUtc);
    }

    public void TimeOutAttempt(
        Guid attemptId,
        string changedBy,
        DateTimeOffset finishedAtUtc)
    {
        var attempt = FindAttempt(attemptId);
        attempt.TimeOut(finishedAtUtc);
        MarkFailed(
            "The provider request timed out.",
            changedBy,
            finishedAtUtc);
    }

    private static string NormalizeNumber(string number)
    {
        var normalized = number?.Trim().ToUpperInvariant() ?? string.Empty;

        if (normalized.Length is < 5 or > 40 ||
            !normalized.StartsWith("ORD-", StringComparison.Ordinal))
        {
            throw new DomainRuleException(
                "Order number must start with ORD- and contain at most 40 characters.");
        }

        return normalized;
    }

    private static decimal PositiveAmount(decimal amount)
    {
        if (amount <= 0)
        {
            throw new DomainRuleException("Order amount must be greater than zero.");
        }

        return amount;
    }

    private static string? NormalizeNotes(string? notes)
    {
        var normalized = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();

        if (normalized?.Length > 1_000)
        {
            throw new DomainRuleException(
                "Order notes must contain at most 1000 characters.");
        }

        return normalized;
    }

    private IntegrationAttempt FindAttempt(Guid attemptId) =>
        _integrationAttempts.FirstOrDefault(attempt => attempt.Id == attemptId)
        ?? throw new DomainRuleException("Integration attempt was not found.");

    private void TransitionTo(
        OrderStatus newStatus,
        string reason,
        string changedBy,
        DateTimeOffset changedAtUtc)
    {
        if (!IsValidTransition(Status, newStatus))
        {
            throw new DomainRuleException(
                $"Transition from {Status} to {newStatus} is not allowed.");
        }

        var previousStatus = Status;
        Status = newStatus;
        UpdatedAtUtc = changedAtUtc;

        _statusHistory.Add(new OrderStatusHistory(
            Guid.NewGuid(),
            Id,
            previousStatus,
            newStatus,
            RequiredAuditText(reason, "Transition reason"),
            RequiredAuditText(changedBy, "Changed by"),
            changedAtUtc));
    }

    private static bool IsValidTransition(
        OrderStatus currentStatus,
        OrderStatus newStatus) =>
        (currentStatus, newStatus) switch
        {
            (OrderStatus.Draft, OrderStatus.Pending) => true,
            (OrderStatus.Draft, OrderStatus.Cancelled) => true,
            (OrderStatus.Pending, OrderStatus.Processing) => true,
            (OrderStatus.Pending, OrderStatus.Cancelled) => true,
            (OrderStatus.Processing, OrderStatus.Completed) => true,
            (OrderStatus.Processing, OrderStatus.Failed) => true,
            (OrderStatus.Failed, OrderStatus.Processing) => true,
            _ => false
        };

    private static string RequiredAuditText(string value, string fieldName)
    {
        var normalized = value?.Trim() ?? string.Empty;

        if (normalized.Length is 0 or > 500)
        {
            throw new DomainRuleException(
                $"{fieldName} must contain between 1 and 500 characters.");
        }

        return normalized;
    }
}
