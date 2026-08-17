using OpsFlow.Application.Orders.Messaging;

namespace OpsFlow.Application.Orders.Ports;

public interface IOrderUpdatePublisher
{
    Task PublishAsync(
        OrderUpdatedMessage message,
        CancellationToken cancellationToken);
}
