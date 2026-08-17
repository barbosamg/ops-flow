using Microsoft.AspNetCore.SignalR;
using OpsFlow.Api.Hubs;
using OpsFlow.Infrastructure.Messaging;

namespace OpsFlow.Api.Background;

public sealed partial class OrderUpdateSignalRRelay(
    AzureOrderQueueClient queueClient,
    IHubContext<OrderUpdatesHub> hubContext,
    ILogger<OrderUpdateSignalRRelay> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var message = await queueClient.ReceiveUpdateAsync(stoppingToken);

                if (message is null)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(500), stoppingToken);
                    continue;
                }

                await hubContext.Clients
                    .Group(OrderUpdatesHub.OrdersGroupName)
                    .SendAsync("OrderUpdated", message.Body, stoppingToken);

                await queueClient.CompleteUpdateAsync(message, stoppingToken);
            }
            catch (Exception exception) when (
                exception is not OperationCanceledException)
            {
                LogRelayFailure(logger, exception);
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
        }
    }

    [LoggerMessage(
        EventId = 2002,
        Level = LogLevel.Warning,
        Message = "Order update relay failed and will retry.")]
    private static partial void LogRelayFailure(
        ILogger logger,
        Exception exception);
}
