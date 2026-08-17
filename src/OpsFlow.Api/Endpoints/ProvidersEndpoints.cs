
using Microsoft.AspNetCore.Http.HttpResults;
using OpsFlow.Application.Providers.Ports;
using OpsFlow.Application.Providers.Queries.GetProviderOptions;

namespace OpsFlow.Api.Endpoints;

public static class ProvidersEndpoints
{
    public static RouteGroupBuilder MapProvidersEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/providers")
            .WithTags("Providers");

        group.MapGet("", GetProvidersAsync)
            .WithName("GetProviders");

        return group;
    }

    private static async Task<
        Ok<IReadOnlyList<ProviderOptionDto>>> GetProvidersAsync(
            IProviderReadRepository repository,
            CancellationToken cancellationToken)
    {
        var providers = await repository.GetActiveAsync(
            cancellationToken);

        return TypedResults.Ok(providers);
    }
}