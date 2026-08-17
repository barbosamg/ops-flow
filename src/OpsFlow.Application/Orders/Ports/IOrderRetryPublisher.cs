using OpsFlow.Application.Orders.Messaging;

namespace OpsFlow.Application.Orders.Ports;

public interface IOrderRetryPublisher
{
    Task PublishAsync(
        OrderRetryMessage message,
        CancellationToken cancellationToken);
}
