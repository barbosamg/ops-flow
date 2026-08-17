using OpsFlow.Application.Common.Exceptions;
using OpsFlow.Application.Orders.Integration;
using OpsFlow.Application.Orders.Messaging;
using OpsFlow.Application.Orders.Ports;
using OpsFlow.Domain.Orders;

namespace OpsFlow.Application.Orders.Services;

public sealed class OrderRetryProcessor(
    IOrderRepository repository,
    IOrderProviderGateway providerGateway,
    IOrderUpdatePublisher updatePublisher,
    TimeProvider timeProvider)
{
    public async Task ProcessAsync(
        OrderRetryMessage message,
        CancellationToken cancellationToken)
    {
        var order = await repository.GetAsync(
            message.OrderId,
            cancellationToken)
            ?? throw new ResourceNotFoundException("Order", message.OrderId);

        var attempt = order.IntegrationAttempts.SingleOrDefault(
            item => item.Id == message.AttemptId)
            ?? throw new ResourceNotFoundException(
                "Integration attempt",
                message.AttemptId);

        if (!attempt.IsActive)
        {
            return;
        }

        if (attempt.Status == IntegrationAttemptStatus.Queued)
        {
            order.StartAttempt(
                attempt.Id,
                "opsflow-worker",
                timeProvider.GetUtcNow());
            await repository.SaveChangesAsync(order, null, cancellationToken);
            await PublishUpdateAsync(order, attempt, cancellationToken);
        }

        var result = await providerGateway.ProcessAsync(
            new ProviderProcessingRequest(
                order.Id,
                order.ProviderId,
                order.Number,
                order.Amount,
                attempt.AttemptNumber,
                attempt.CorrelationId),
            cancellationToken);

        var finishedAtUtc = timeProvider.GetUtcNow();

        switch (result.Outcome)
        {
            case ProviderProcessingOutcome.Succeeded:
                order.CompleteAttempt(
                    attempt.Id,
                    "opsflow-worker",
                    finishedAtUtc);
                break;

            case ProviderProcessingOutcome.Rejected:
                order.FailAttempt(
                    attempt.Id,
                    result.ErrorCode ?? "PROVIDER_REJECTED",
                    result.SanitizedError ?? "The provider rejected the order.",
                    "opsflow-worker",
                    finishedAtUtc);
                break;

            case ProviderProcessingOutcome.TimedOut:
                order.TimeOutAttempt(
                    attempt.Id,
                    "opsflow-worker",
                    finishedAtUtc);
                break;

            default:
                throw new InvalidOperationException(
                    $"Unsupported provider outcome '{result.Outcome}'.");
        }

        await repository.SaveChangesAsync(order, null, cancellationToken);
        await PublishUpdateAsync(order, attempt, cancellationToken);
    }

    public async Task MarkDeliveryExhaustedAsync(
        OrderRetryMessage message,
        CancellationToken cancellationToken)
    {
        var order = await repository.GetAsync(
            message.OrderId,
            cancellationToken)
            ?? throw new ResourceNotFoundException("Order", message.OrderId);
        var attempt = order.IntegrationAttempts.SingleOrDefault(
            item => item.Id == message.AttemptId)
            ?? throw new ResourceNotFoundException(
                "Integration attempt",
                message.AttemptId);

        if (!attempt.IsActive)
        {
            return;
        }

        var now = timeProvider.GetUtcNow();

        if (attempt.Status == IntegrationAttemptStatus.Queued)
        {
            order.StartAttempt(attempt.Id, "opsflow-worker", now);
        }

        order.FailAttempt(
            attempt.Id,
            "DELIVERY_EXHAUSTED",
            "Processing exceeded the configured delivery attempts.",
            "opsflow-worker",
            timeProvider.GetUtcNow());

        await repository.SaveChangesAsync(order, null, cancellationToken);
        await PublishUpdateAsync(order, attempt, cancellationToken);
    }

    private Task PublishUpdateAsync(
        Order order,
        IntegrationAttempt attempt,
        CancellationToken cancellationToken) =>
        updatePublisher.PublishAsync(
            new OrderUpdatedMessage(
                order.Id,
                order.Status,
                attempt.Id,
                attempt.Status,
                attempt.CorrelationId,
                timeProvider.GetUtcNow()),
            cancellationToken);
}
