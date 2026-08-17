using OpsFlow.Web.Models.Common;
using OpsFlow.Web.Models.Orders;

namespace OpsFlow.Web.Clients.Orders;

public interface IOrdersApiClient
{
    Task<PagedResponse<OrderListItem>> GetOrdersAsync(
        OrderSearchRequest request,
        CancellationToken cancellationToken);

    Task<OrderDetails> GetOrderAsync(
        Guid orderId,
        CancellationToken cancellationToken);

    Task<OrderDetails> CreateOrderAsync(
        OrderUpsertRequest request,
        CancellationToken cancellationToken);

    Task<OrderDetails> UpdateOrderAsync(
        Guid orderId,
        OrderUpsertRequest request,
        CancellationToken cancellationToken);

    Task<OrderRetryAccepted> RetryOrderAsync(
        Guid orderId,
        string idempotencyKey,
        CancellationToken cancellationToken);
}
