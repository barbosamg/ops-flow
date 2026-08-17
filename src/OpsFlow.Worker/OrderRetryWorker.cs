using OpsFlow.Application.Orders.Services;
using OpsFlow.Application.Orders.Messaging;
using OpsFlow.Infrastructure.Messaging;

namespace OpsFlow.Worker;

public sealed partial class OrderRetryWorker(
    AzureOrderQueueClient queueClient,
    IServiceScopeFactory scopeFactory,
    ILogger<OrderRetryWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await queueClient.EnsureQueuesAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var message = await queueClient.ReceiveRetryAsync(stoppingToken);

            if (message is null)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(500), stoppingToken);
                continue;
            }

            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var processor = scope.ServiceProvider
                    .GetRequiredService<OrderRetryProcessor>();

                await processor.ProcessAsync(message.Body, stoppingToken);
                await queueClient.CompleteRetryAsync(message, stoppingToken);

                LogProcessed(
                    logger,
                    message.Body.OrderId,
                    message.Body.AttemptId,
                    message.Body.CorrelationId);
            }
            catch (Exception exception) when (
                exception is not OperationCanceledException)
            {
                await HandleFailureAsync(message, exception, stoppingToken);
            }
        }
    }

    private async Task HandleFailureAsync(
        DequeuedQueueMessage<OrderRetryMessage> message,
        Exception exception,
        CancellationToken cancellationToken)
    {
        LogFailure(
            logger,
            message.Body.OrderId,
            message.DequeueCount,
            exception);

        if (message.DequeueCount >= queueClient.Options.MaxDeliveryAttempts)
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            await scope.ServiceProvider
                .GetRequiredService<OrderRetryProcessor>()
                .MarkDeliveryExhaustedAsync(message.Body, cancellationToken);
            await queueClient.MoveToPoisonAsync(message, cancellationToken);
            LogPoisoned(logger, message.Body.OrderId, message.MessageId);
            return;
        }

        var exponent = Math.Min(message.DequeueCount, 6);
        var delay = TimeSpan.FromSeconds(Math.Pow(2, exponent));
        await queueClient.AbandonRetryAsync(
            message,
            delay,
            cancellationToken);
    }

    [LoggerMessage(
        EventId = 3001,
        Level = LogLevel.Information,
        Message = "Processed retry for order {OrderId}, attempt {AttemptId}, correlation {CorrelationId}.")]
    private static partial void LogProcessed(
        ILogger logger,
        Guid orderId,
        Guid attemptId,
        string correlationId);

    [LoggerMessage(
        EventId = 3002,
        Level = LogLevel.Warning,
        Message = "Retry processing failed for order {OrderId} on delivery {DeliveryCount}.")]
    private static partial void LogFailure(
        ILogger logger,
        Guid orderId,
        long deliveryCount,
        Exception exception);

    [LoggerMessage(
        EventId = 3003,
        Level = LogLevel.Error,
        Message = "Retry for order {OrderId} was moved to poison queue as message {MessageId}.")]
    private static partial void LogPoisoned(
        ILogger logger,
        Guid orderId,
        string messageId);
}
