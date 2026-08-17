using OpsFlow.Application.Orders.Queries.GetOrderDetails;

namespace OpsFlow.Application.Orders.Ports;

public interface IOrderDetailsReadRepository
{
    Task<OrderDetailsDto?> GetDetailsAsync(
        Guid id,
        CancellationToken cancellationToken);
}
