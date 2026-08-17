using OpsFlow.Application.Orders.Integration;

namespace OpsFlow.Api.Endpoints;

public static class SimulatedProviderEndpoints
{
    public static RouteGroupBuilder MapSimulatedProviderEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/simulated-provider")
            .WithTags("Simulated Provider");

        group.MapPost("/process", ProcessAsync)
            .WithName("ProcessSimulatedProviderOrder");

        return group;
    }

    private static async Task<IResult> ProcessAsync(
        ProviderProcessingRequest request,
        CancellationToken cancellationToken)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(300), cancellationToken);

        var marker = GetScenarioMarker(request);

        return marker switch
        {
            0 => await SimulateTimeoutAsync(cancellationToken),
            1 => TypedResults.UnprocessableEntity(new
            {
                ErrorCode = "PROVIDER_REJECTED",
                SanitizedError = "The provider rejected the order."
            }),
            2 => TypedResults.StatusCode(StatusCodes.Status503ServiceUnavailable),
            _ => TypedResults.Ok(new { Status = "Accepted" })
        };
    }

    private static int GetScenarioMarker(ProviderProcessingRequest request)
    {
        if (request.CorrelationId.StartsWith("timeout-", StringComparison.Ordinal))
        {
            return 0;
        }

        if (request.CorrelationId.StartsWith("reject-", StringComparison.Ordinal))
        {
            return 1;
        }

        if (request.CorrelationId.StartsWith("transient-", StringComparison.Ordinal))
        {
            return 2;
        }

        if (request.CorrelationId.StartsWith("success-", StringComparison.Ordinal))
        {
            return 3;
        }

        return Convert.ToInt32(
            request.OrderId.ToString("N")[^1].ToString(),
            16) % 4;
    }

    private static async Task<IResult> SimulateTimeoutAsync(
        CancellationToken cancellationToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(15), cancellationToken);
        return TypedResults.Ok(new { Status = "Accepted" });
    }
}
