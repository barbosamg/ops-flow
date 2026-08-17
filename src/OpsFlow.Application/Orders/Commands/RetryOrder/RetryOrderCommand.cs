namespace OpsFlow.Application.Orders.Commands.RetryOrder;

public sealed record RetryOrderCommand(string IdempotencyKey);
