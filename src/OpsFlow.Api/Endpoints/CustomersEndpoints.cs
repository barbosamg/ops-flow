using Microsoft.AspNetCore.Http.HttpResults;
using OpsFlow.Application.Customers.Ports;
using OpsFlow.Application.Customers.Queries.GetCustomerOptions;

namespace OpsFlow.Api.Endpoints;

public static class CustomersEndpoints
{
    public static RouteGroupBuilder MapCustomersEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/customers")
            .WithTags("Customers");

        group.MapGet("", GetCustomersAsync)
            .WithName("GetCustomers");

        return group;
    }

    private static async Task<Ok<IReadOnlyList<CustomerOptionDto>>>
        GetCustomersAsync(
            ICustomerReadRepository repository,
            CancellationToken cancellationToken) =>
        TypedResults.Ok(await repository.GetOptionsAsync(cancellationToken));
}
