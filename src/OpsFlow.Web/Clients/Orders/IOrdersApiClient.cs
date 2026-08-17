using OpsFlow.Web.Models.Common;
using OpsFlow.Web.Models.Orders;

namespace OpsFlow.Web.Clients.Orders;

public interface IOrdersApiClient
{
    Task<PagedResponse<OrderListItem>> GetOrdersAsync(
        OrderSearchRequest request,
        CancellationToken cancellationToken);
}
