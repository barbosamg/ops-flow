namespace OpsFlow.Application.Orders.Commands.CreateOrder;

public sealed record CreateOrderCommand(
    Guid CustomerId,
    Guid ProviderId,
    decimal Amount,
    string? Notes);
