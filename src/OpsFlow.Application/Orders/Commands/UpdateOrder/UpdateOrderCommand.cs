namespace OpsFlow.Application.Orders.Commands.UpdateOrder;

public sealed record UpdateOrderCommand(
    Guid CustomerId,
    Guid ProviderId,
    decimal Amount,
    string? Notes,
    string RowVersion);
