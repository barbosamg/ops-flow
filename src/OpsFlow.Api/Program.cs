using System.Text.Json.Serialization;
using FluentValidation;
using OpsFlow.Api.Endpoints;
using OpsFlow.Application.Orders.Ports;
using OpsFlow.Application.Orders.Queries.GetOrders;
using OpsFlow.Domain.Orders;
using OpsFlow.Infrastructure.Orders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<
    IValidator<GetOrdersQuery>,
    GetOrdersQueryValidator>();

builder.Services.AddSingleton<
    IOrderReadRepository,
    InMemoryOrderReadRepository>();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(
        new JsonStringEnumConverter<OrderStatus>());
});

var app = builder.Build();

app.MapGet("/", () => "OpsFlow API");

app.MapOrdersEndpoints();

app.Run();
