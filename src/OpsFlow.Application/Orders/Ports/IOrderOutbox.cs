using OpsFlow.Application.Orders.Messaging;

namespace OpsFlow.Application.Orders.Ports;

public interface IOrderOutbox
{
    void AddOrderRetryRequested(OrderRetryMessage message);
}
