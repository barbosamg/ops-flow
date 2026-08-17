namespace OpsFlow.Infrastructure.Messaging;

public sealed class OrderQueueOptions
{
    public const string SectionName = "Queues";

    public string ConnectionString { get; set; } = "UseDevelopmentStorage=true";

    public string RetryQueueName { get; set; } = "order-retries";

    public string PoisonQueueName { get; set; } = "order-retries-poison";

    public string UpdateQueueName { get; set; } = "order-updates";

    public int VisibilityTimeoutSeconds { get; set; } = 30;

    public int MaxDeliveryAttempts { get; set; } = 5;
}
