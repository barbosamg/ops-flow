using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpsFlow.Infrastructure.Messaging;

namespace OpsFlow.Api.Health;

public sealed class QueueHealthCheck(AzureOrderQueueClient queueClient) :
    IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default) =>
        await queueClient.CanConnectAsync(cancellationToken)
            ? HealthCheckResult.Healthy("Azure Storage Queue is reachable.")
            : HealthCheckResult.Unhealthy("Azure Storage Queue is unavailable.");
}
