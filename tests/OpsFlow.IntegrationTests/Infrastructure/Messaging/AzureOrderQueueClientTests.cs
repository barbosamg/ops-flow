using Microsoft.Extensions.Options;
using OpsFlow.Infrastructure.Messaging;

namespace OpsFlow.IntegrationTests.Infrastructure.Messaging;

public sealed class AzureOrderQueueClientTests
{
    [Fact]
    public async Task EnsureQueuesAsyncShouldCreateAzuriteQueues()
    {
        var client = new AzureOrderQueueClient(Options.Create(
            new OrderQueueOptions
            {
                ConnectionString = "UseDevelopmentStorage=true",
                RetryQueueName = $"test-retries-{Guid.NewGuid():N}",
                PoisonQueueName = $"test-poison-{Guid.NewGuid():N}",
                UpdateQueueName = $"test-updates-{Guid.NewGuid():N}"
            }));

        await client.EnsureQueuesAsync(CancellationToken.None);
    }
}
