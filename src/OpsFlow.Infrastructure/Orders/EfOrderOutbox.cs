using System.Text.Json;
using OpsFlow.Application.Orders.Messaging;
using OpsFlow.Application.Orders.Ports;
using OpsFlow.Infrastructure.Persistence;
using OpsFlow.Infrastructure.Persistence.Outbox;

namespace OpsFlow.Infrastructure.Orders;

public sealed class EfOrderOutbox(
    OpsFlowDbContext dbContext,
    TimeProvider timeProvider) : IOrderOutbox
{
    public void AddOrderRetryRequested(OrderRetryMessage message)
    {
        dbContext.OutboxMessages.Add(new OutboxMessage(
            Guid.NewGuid(),
            nameof(OrderRetryMessage),
            JsonSerializer.Serialize(message),
            timeProvider.GetUtcNow()));
    }
}
