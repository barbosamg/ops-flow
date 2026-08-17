using Microsoft.AspNetCore.Http.HttpResults;
using OpsFlow.Application.Dashboard.Ports;
using OpsFlow.Application.Dashboard.Queries.GetDashboardSummary;

namespace OpsFlow.Api.Endpoints;

public static class DashboardEndpoints
{
    public static RouteGroupBuilder MapDashboardEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/dashboard")
            .WithTags("Dashboard");

        group.MapGet("/summary", GetSummaryAsync)
            .WithName("GetDashboardSummary");

        return group;
    }

    private static async Task<Ok<DashboardSummaryDto>> GetSummaryAsync(
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        IDashboardReadRepository repository,
        CancellationToken cancellationToken) =>
        TypedResults.Ok(await repository.GetSummaryAsync(
            fromUtc,
            toUtc,
            cancellationToken));
}
