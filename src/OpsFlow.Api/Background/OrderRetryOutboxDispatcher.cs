using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OpsFlow.Application.Orders.Messaging;
using OpsFlow.Application.Orders.Ports;
using OpsFlow.Infrastructure.Persistence;

namespace OpsFlow.Api.Background;

public sealed partial class OrderRetryOutboxDispatcher(
    IDbContextFactory<OpsFlowDbContext> dbContextFactory,
    IOrderRetryPublisher publisher,
    TimeProvider timeProvider,
    ILogger<OrderRetryOutboxDispatcher> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var dispatched = await DispatchBatchAsync(stoppingToken);

            if (dispatched is 0)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }
    }

    private async Task<int> DispatchBatchAsync(CancellationToken cancellationToken)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(
            cancellationToken);

        var messages = await dbContext.OutboxMessages
            .Where(message => message.ProcessedAtUtc == null)
            .OrderBy(message => message.OccurredAtUtc)
            .Take(20)
            .ToArrayAsync(cancellationToken);

        var dispatched = 0;

        foreach (var outboxMessage in messages)
        {
            try
            {
                var message = JsonSerializer.Deserialize<OrderRetryMessage>(
                    outboxMessage.Payload)
                    ?? throw new JsonException("Outbox payload is empty.");

                await publisher.PublishAsync(message, cancellationToken);
                outboxMessage.MarkProcessed(timeProvider.GetUtcNow());
                dispatched++;
            }
            catch (Exception exception) when (
                exception is not OperationCanceledException)
            {
                outboxMessage.MarkFailed(exception.Message);
                LogDispatchFailure(
                    logger,
                    outboxMessage.Id,
                    outboxMessage.DeliveryAttempts,
                    exception);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return dispatched;
    }

    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Warning,
        Message = "Outbox message {MessageId} delivery attempt {DeliveryAttempt} failed.")]
    private static partial void LogDispatchFailure(
        ILogger logger,
        Guid messageId,
        int deliveryAttempt,
        Exception exception);
}
