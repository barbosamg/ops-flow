
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using OpsFlow.Api.Contracts.Orders;
using OpsFlow.Application.Common.Pagination;
using OpsFlow.Application.Orders.Queries.GetOrders;
using OpsFlow.Application.Orders.Ports;

namespace OpsFlow.Api.Endpoints;

public static class OrdersEndpoints
{
    public static RouteGroupBuilder MapOrdersEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/orders")
            .WithTags("Orders");

        group.MapGet("", GetOrdersAsync)
            .WithName("GetOrders");

        return group;
    }

    private static async Task<
        Results<Ok<PagedResult<OrderListItemDto>>, ValidationProblem>>
        GetOrdersAsync(
            [AsParameters] GetOrdersRequest request,
            IValidator<GetOrdersQuery> validator,
            IOrderReadRepository repository,
            CancellationToken cancellationToken)
    {
        var query = request.ToQuery();

        var validationResult = await validator.ValidateAsync(
            query,
            cancellationToken);

        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(
                validationResult.ToDictionary());
        }

        var response = await repository.SearchAsync(
            query,
            cancellationToken);

        return TypedResults.Ok(response);
    }
}
