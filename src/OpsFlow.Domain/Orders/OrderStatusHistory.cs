namespace OpsFlow.Domain.Orders;

public sealed class OrderStatusHistory
{
    private OrderStatusHistory()
    {
    }

    internal OrderStatusHistory(
        Guid id,
        Guid orderId,
        OrderStatus previousStatus,
        OrderStatus newStatus,
        string reason,
        string changedBy,
        DateTimeOffset changedAtUtc)
    {
        Id = id;
        OrderId = orderId;
        PreviousStatus = previousStatus;
        NewStatus = newStatus;
        Reason = reason;
        ChangedBy = changedBy;
        ChangedAtUtc = changedAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid OrderId { get; private set; }

    public OrderStatus PreviousStatus { get; private set; }

    public OrderStatus NewStatus { get; private set; }

    public string Reason { get; private set; } = string.Empty;

    public string ChangedBy { get; private set; } = string.Empty;

    public DateTimeOffset ChangedAtUtc { get; private set; }
}
