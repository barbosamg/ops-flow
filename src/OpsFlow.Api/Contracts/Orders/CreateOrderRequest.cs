using OpsFlow.Application.Orders.Commands.CreateOrder;

namespace OpsFlow.Api.Contracts.Orders;

public sealed record CreateOrderRequest(
    Guid CustomerId,
    Guid ProviderId,
    decimal Amount,
    string? Notes)
{
    public CreateOrderCommand ToCommand() =>
        new(CustomerId, ProviderId, Amount, Notes);
}
