using System.Text.Json;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.Extensions.Options;
using OpsFlow.Application.Orders.Messaging;
using OpsFlow.Application.Orders.Ports;

namespace OpsFlow.Infrastructure.Messaging;

public sealed class AzureOrderQueueClient :
    IOrderRetryPublisher,
    IOrderUpdatePublisher
{
    private readonly QueueClient _poisonQueue;
    private readonly QueueClient _retryQueue;
    private readonly QueueClient _updateQueue;

    public AzureOrderQueueClient(IOptions<OrderQueueOptions> options)
    {
        Options = options.Value;

        var clientOptions = new QueueClientOptions
        {
            MessageEncoding = QueueMessageEncoding.Base64
        };

        _retryQueue = new QueueClient(
            Options.ConnectionString,
            Options.RetryQueueName,
            clientOptions);
        _poisonQueue = new QueueClient(
            Options.ConnectionString,
            Options.PoisonQueueName,
            clientOptions);
        _updateQueue = new QueueClient(
            Options.ConnectionString,
            Options.UpdateQueueName,
            clientOptions);
    }

    public OrderQueueOptions Options { get; }

    public async Task EnsureQueuesAsync(CancellationToken cancellationToken)
    {
        await _retryQueue.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
        await _poisonQueue.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
        await _updateQueue.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
    }

    async Task IOrderRetryPublisher.PublishAsync(
        OrderRetryMessage message,
        CancellationToken cancellationToken)
    {
        await EnsureQueuesAsync(cancellationToken);
        await _retryQueue.SendMessageAsync(
            JsonSerializer.Serialize(message),
            cancellationToken);
    }

    async Task IOrderUpdatePublisher.PublishAsync(
        OrderUpdatedMessage message,
        CancellationToken cancellationToken)
    {
        await EnsureQueuesAsync(cancellationToken);
        await _updateQueue.SendMessageAsync(
            JsonSerializer.Serialize(message),
            cancellationToken);
    }

    public async Task<DequeuedQueueMessage<OrderRetryMessage>?>
        ReceiveRetryAsync(CancellationToken cancellationToken)
    {
        await EnsureQueuesAsync(cancellationToken);
        var response = await _retryQueue.ReceiveMessageAsync(
            TimeSpan.FromSeconds(Options.VisibilityTimeoutSeconds),
            cancellationToken);

        return response.Value is null
            ? null
            : Deserialize<OrderRetryMessage>(response.Value);
    }

    public async Task<DequeuedQueueMessage<OrderUpdatedMessage>?>
        ReceiveUpdateAsync(CancellationToken cancellationToken)
    {
        await EnsureQueuesAsync(cancellationToken);
        var response = await _updateQueue.ReceiveMessageAsync(
            TimeSpan.FromSeconds(Options.VisibilityTimeoutSeconds),
            cancellationToken);

        return response.Value is null
            ? null
            : Deserialize<OrderUpdatedMessage>(response.Value);
    }

    public Task CompleteRetryAsync(
        DequeuedQueueMessage<OrderRetryMessage> message,
        CancellationToken cancellationToken) =>
        _retryQueue.DeleteMessageAsync(
            message.MessageId,
            message.PopReceipt,
            cancellationToken);

    public Task CompleteUpdateAsync(
        DequeuedQueueMessage<OrderUpdatedMessage> message,
        CancellationToken cancellationToken) =>
        _updateQueue.DeleteMessageAsync(
            message.MessageId,
            message.PopReceipt,
            cancellationToken);

    public async Task AbandonRetryAsync(
        DequeuedQueueMessage<OrderRetryMessage> message,
        TimeSpan delay,
        CancellationToken cancellationToken)
    {
        await _retryQueue.UpdateMessageAsync(
            message.MessageId,
            message.PopReceipt,
            message.RawBody,
            delay,
            cancellationToken);
    }

    public async Task MoveToPoisonAsync(
        DequeuedQueueMessage<OrderRetryMessage> message,
        CancellationToken cancellationToken)
    {
        await _poisonQueue.SendMessageAsync(
            message.RawBody,
            cancellationToken);
        await CompleteRetryAsync(message, cancellationToken);
    }

    public async Task<bool> CanConnectAsync(CancellationToken cancellationToken)
    {
        try
        {
            await EnsureQueuesAsync(cancellationToken);
            await _retryQueue.GetPropertiesAsync(cancellationToken);
            return true;
        }
        catch (Azure.RequestFailedException)
        {
            return false;
        }
    }

    private static DequeuedQueueMessage<T> Deserialize<T>(QueueMessage message)
    {
        var rawBody = message.Body.ToString();
        var body = JsonSerializer.Deserialize<T>(rawBody)
            ?? throw new JsonException(
                $"Queue message '{message.MessageId}' contains an empty payload.");

        return new DequeuedQueueMessage<T>(
            message.MessageId,
            message.PopReceipt,
            message.DequeueCount,
            rawBody,
            body);
    }
}
