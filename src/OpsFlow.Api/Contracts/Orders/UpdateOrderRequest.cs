using OpsFlow.Application.Orders.Commands.UpdateOrder;

namespace OpsFlow.Api.Contracts.Orders;

public sealed record UpdateOrderRequest(
    Guid CustomerId,
    Guid ProviderId,
    decimal Amount,
    string? Notes,
    string RowVersion)
{
    public UpdateOrderCommand ToCommand() =>
        new(CustomerId, ProviderId, Amount, Notes, RowVersion);
}
