
using OpsFlow.Application.Common.Pagination;
using OpsFlow.Application.Orders.Queries.GetOrders;

namespace OpsFlow.Application.Orders.Ports;

public interface IOrderReadRepository
{
    Task<PagedResult<OrderListItemDto>> SearchAsync(
        GetOrdersQuery query,
        CancellationToken cancellationToken);
}