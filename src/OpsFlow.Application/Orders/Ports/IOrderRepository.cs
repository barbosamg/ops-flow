using OpsFlow.Domain.Orders;

namespace OpsFlow.Application.Orders.Ports;

public interface IOrderRepository
{
    Task<Order?> GetAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> IsActiveCustomerAsync(
        Guid customerId,
        CancellationToken cancellationToken);

    Task<bool> IsActiveProviderAsync(
        Guid providerId,
        CancellationToken cancellationToken);

    Task AddAsync(Order order, CancellationToken cancellationToken);

    Task SaveChangesAsync(
        Order order,
        byte[]? expectedRowVersion,
        CancellationToken cancellationToken);
}
