namespace OpsFlow.Application.Orders.Integration;

public interface IOrderProviderGateway
{
    Task<ProviderProcessingResult> ProcessAsync(
        ProviderProcessingRequest request,
        CancellationToken cancellationToken);
}

public sealed record ProviderProcessingRequest(
    Guid OrderId,
    Guid ProviderId,
    string OrderNumber,
    decimal Amount,
    int AttemptNumber,
    string CorrelationId);

public sealed record ProviderProcessingResult(
    ProviderProcessingOutcome Outcome,
    string? ErrorCode,
    string? SanitizedError)
{
    public static ProviderProcessingResult Succeeded() =>
        new(ProviderProcessingOutcome.Succeeded, null, null);

    public static ProviderProcessingResult Rejected(
        string errorCode,
        string sanitizedError) =>
        new(ProviderProcessingOutcome.Rejected, errorCode, sanitizedError);

    public static ProviderProcessingResult TimedOut() =>
        new(
            ProviderProcessingOutcome.TimedOut,
            "PROVIDER_TIMEOUT",
            "The provider did not respond within the configured timeout.");
}

public enum ProviderProcessingOutcome
{
    Succeeded = 0,
    Rejected = 1,
    TimedOut = 2
}
