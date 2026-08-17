namespace OpsFlow.Domain.Orders;

public enum OrderStatus
{
    Draft = 0,
    Pending = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4,
    Cancelled = 5
}
