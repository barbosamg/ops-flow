
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using OpsFlow.Api.Contracts.Orders;
using OpsFlow.Application.Common.Pagination;
using OpsFlow.Application.Orders.Queries.GetOrders;
using OpsFlow.Application.Orders.Ports;
using OpsFlow.Application.Orders.Commands.RetryOrder;
using OpsFlow.Application.Orders.Queries.GetOrderDetails;
using OpsFlow.Application.Orders.Services;

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

        group.MapGet("/{id:guid}", GetOrderAsync)
            .WithName("GetOrder");

        group.MapPost("", CreateOrderAsync)
            .WithName("CreateOrder");

        group.MapPut("/{id:guid}", UpdateOrderAsync)
            .WithName("UpdateOrder");

        group.MapPost("/{id:guid}/retry", RetryOrderAsync)
            .WithName("RetryOrder");

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

    private static async Task<Ok<OrderDetailsDto>> GetOrderAsync(
        Guid id,
        OrderApplicationService service,
        CancellationToken cancellationToken) =>
        TypedResults.Ok(await service.GetAsync(id, cancellationToken));

    private static async Task<Created<OrderDetailsDto>> CreateOrderAsync(
        CreateOrderRequest request,
        OrderApplicationService service,
        CancellationToken cancellationToken)
    {
        var order = await service.CreateAsync(
            request.ToCommand(),
            "demo-operator",
            cancellationToken);

        return TypedResults.Created($"/api/orders/{order.Id}", order);
    }

    private static async Task<Ok<OrderDetailsDto>> UpdateOrderAsync(
        Guid id,
        UpdateOrderRequest request,
        OrderApplicationService service,
        CancellationToken cancellationToken) =>
        TypedResults.Ok(await service.UpdateAsync(
            id,
            request.ToCommand(),
            cancellationToken));

    private static async Task<Accepted<OrderRetryAcceptedDto>> RetryOrderAsync(
        Guid id,
        HttpRequest request,
        OrderApplicationService service,
        CancellationToken cancellationToken)
    {
        var idempotencyKey = request.Headers["Idempotency-Key"].ToString();
        var result = await service.RetryAsync(
            id,
            new RetryOrderCommand(idempotencyKey),
            cancellationToken);

        return TypedResults.Accepted($"/api/orders/{id}", result);
    }
}
