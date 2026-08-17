namespace OpsFlow.Infrastructure.Messaging;

public sealed record DequeuedQueueMessage<T>(
    string MessageId,
    string PopReceipt,
    long DequeueCount,
    string RawBody,
    T Body);
