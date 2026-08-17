using FluentValidation;
using OpsFlow.Application.Common.Exceptions;
using OpsFlow.Application.Orders.Commands.CreateOrder;
using OpsFlow.Application.Orders.Commands.RetryOrder;
using OpsFlow.Application.Orders.Commands.UpdateOrder;
using OpsFlow.Application.Orders.Messaging;
using OpsFlow.Application.Orders.Ports;
using OpsFlow.Application.Orders.Queries.GetOrderDetails;
using OpsFlow.Domain.Orders;

namespace OpsFlow.Application.Orders.Services;

public sealed class OrderApplicationService(
    IValidator<CreateOrderCommand> createValidator,
    IValidator<UpdateOrderCommand> updateValidator,
    IValidator<RetryOrderCommand> retryValidator,
    IOrderRepository repository,
    IOrderDetailsReadRepository detailsReader,
    IOrderOutbox outbox,
    TimeProvider timeProvider)
{
    public async Task<OrderDetailsDto> GetAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        await detailsReader.GetDetailsAsync(id, cancellationToken)
        ?? throw new ResourceNotFoundException("Order", id);

    public async Task<OrderDetailsDto> CreateAsync(
        CreateOrderCommand command,
        string actor,
        CancellationToken cancellationToken)
    {
        await ValidateAsync(createValidator, command, cancellationToken);
        await EnsureActiveReferencesAsync(
            command.CustomerId,
            command.ProviderId,
            cancellationToken);

        var now = timeProvider.GetUtcNow();
        var order = Order.Create(
            Guid.NewGuid(),
            $"ORD-{now:yyyy}-{Guid.NewGuid():N}"[..17].ToUpperInvariant(),
            command.CustomerId,
            command.ProviderId,
            command.Amount,
            command.Notes,
            now);
        order.Submit(actor, now);

        await repository.AddAsync(order, cancellationToken);
        await repository.SaveChangesAsync(order, null, cancellationToken);

        return await GetAsync(order.Id, cancellationToken);
    }

    public async Task<OrderDetailsDto> UpdateAsync(
        Guid id,
        UpdateOrderCommand command,
        CancellationToken cancellationToken)
    {
        await ValidateAsync(updateValidator, command, cancellationToken);
        await EnsureActiveReferencesAsync(
            command.CustomerId,
            command.ProviderId,
            cancellationToken);

        var order = await repository.GetAsync(id, cancellationToken)
            ?? throw new ResourceNotFoundException("Order", id);

        order.Update(
            command.CustomerId,
            command.ProviderId,
            command.Amount,
            command.Notes,
            timeProvider.GetUtcNow());

        await repository.SaveChangesAsync(
            order,
            Convert.FromBase64String(command.RowVersion),
            cancellationToken);

        return await GetAsync(order.Id, cancellationToken);
    }

    public async Task<OrderRetryAcceptedDto> RetryAsync(
        Guid id,
        RetryOrderCommand command,
        CancellationToken cancellationToken)
    {
        await ValidateAsync(retryValidator, command, cancellationToken);

        var order = await repository.GetAsync(id, cancellationToken)
            ?? throw new ResourceNotFoundException("Order", id);

        var attempt = order.QueueRetry(
            command.IdempotencyKey,
            timeProvider.GetUtcNow());

        outbox.AddOrderRetryRequested(new OrderRetryMessage(
            order.Id,
            attempt.Id,
            attempt.CorrelationId));

        await repository.SaveChangesAsync(order, null, cancellationToken);

        return new OrderRetryAcceptedDto(
            order.Id,
            attempt.Id,
            attempt.AttemptNumber,
            attempt.CorrelationId,
            attempt.Status);
    }

    private async Task EnsureActiveReferencesAsync(
        Guid customerId,
        Guid providerId,
        CancellationToken cancellationToken)
    {
        if (!await repository.IsActiveCustomerAsync(
                customerId,
                cancellationToken))
        {
            throw new ConflictException(
                "The selected customer does not exist or is inactive.");
        }

        if (!await repository.IsActiveProviderAsync(
                providerId,
                cancellationToken))
        {
            throw new ConflictException(
                "The selected provider does not exist or is inactive.");
        }
    }

    private static async Task ValidateAsync<T>(
        IValidator<T> validator,
        T instance,
        CancellationToken cancellationToken)
    {
        var result = await validator.ValidateAsync(instance, cancellationToken);

        if (!result.IsValid)
        {
            throw new ApplicationValidationException(
                new Dictionary<string, string[]>(result.ToDictionary()));
        }
    }
}

public sealed record OrderRetryAcceptedDto(
    Guid OrderId,
    Guid AttemptId,
    int AttemptNumber,
    string CorrelationId,
    IntegrationAttemptStatus Status);
